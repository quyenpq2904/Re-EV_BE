using AutoMapper;
using ReEV.Common.Contracts.Users;
using ReEV.Service.Auth.DTOs;
using ReEV.Service.Auth.Exceptions;
using ReEV.Service.Auth.Models;
using ReEV.Service.Auth.Repositories.Interfaces;
using ReEV.Service.Auth.Services.Interfaces;

namespace ReEV.Service.Auth.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;
        private readonly RabbitMQPublisher _publisher;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository repository, IMapper mapper, RabbitMQPublisher publisher, ILogger<UserService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<PaginationResult<UserDTO>> GetUsers(int page = 1, int pageSize = 10, string search = "")
        {
            var paginatedUsers = await _repository.GetAllAsync(page, pageSize, search);
            var userDtos = _mapper.Map<List<UserDTO>>(paginatedUsers.Items);
            return new PaginationResult<UserDTO>
            {
                Items = userDtos,
                TotalCount = paginatedUsers.TotalCount,
                TotalPages = paginatedUsers.TotalPages,
                Page = paginatedUsers.Page,
                Pagesize = paginatedUsers.Pagesize
            };
        }

        public async Task<UserDTO?> GetUserById(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO?> UpdateUser(Guid id, UserUpdateDTO userUpdateDto)
        {
            var errors = new Dictionary<string, string>();

            // Kiểm tra phone number đã tồn tại chưa (nhưng không phải của user hiện tại)
            var existingUserByPhone = await _repository.GetByPhoneNumberAsync(userUpdateDto.PhoneNumber);
            if (existingUserByPhone != null && existingUserByPhone.Id != id)
            {
                errors["phone_number"] = "Phone number already exists.";
            }

            if (errors.Count > 0)
            {
                throw new ValidationException(errors);
            }

            var userEntity = _mapper.Map<User>(userUpdateDto);

            var updatedUser = await _repository.UpdateAsync(id, userEntity);

            if (updatedUser == null)
            {
                return null;
            }

            // Đảm bảo event luôn được publish khi update thành công
            try
            {
                var userEvent = new UserUpsertedV1(
                    updatedUser.Id, 
                    updatedUser.Email, 
                    updatedUser.FullName, 
                    updatedUser.PhoneNumber, 
                    updatedUser.AvatarUrl
                );
                
                await _publisher.PublishUserUpsertedAsync(userEvent);
                _logger.LogInformation("User update event published successfully. UserId: {UserId}", updatedUser.Id);
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw để không làm fail request
                // Event sẽ được retry hoặc xử lý sau
                _logger.LogError(ex, "Failed to publish user update event. UserId: {UserId}", updatedUser.Id);
            }

            return _mapper.Map<UserDTO>(updatedUser);
        }

        public async Task<UserDTO> CreateUser(UserCreateDTO userCreateDto)
        {
            var userEntity = _mapper.Map<User>(userCreateDto);

            var newUser = await _repository.CreateAsync(userEntity);
            
            // Đảm bảo event luôn được publish khi create thành công
            try
            {
                var userEvent = new UserUpsertedV1(
                    newUser.Id, 
                    newUser.Email, 
                    newUser.FullName, 
                    newUser.PhoneNumber, 
                    newUser.AvatarUrl
                );
                
                await _publisher.PublishUserUpsertedAsync(userEvent);
                _logger.LogInformation("User create event published successfully. UserId: {UserId}", newUser.Id);
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw để không làm fail request
                // Event sẽ được retry hoặc xử lý sau
                _logger.LogError(ex, "Failed to publish user create event. UserId: {UserId}", newUser.Id);
            }

            return _mapper.Map<UserDTO>(newUser);
        }
    }
}
