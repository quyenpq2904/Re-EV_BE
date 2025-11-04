using AutoMapper;
using ReEV.Common.Contracts.Users;
using ReEV.Service.Auth.DTOs;
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

        public UserService(IUserRepository repository, IMapper mapper, RabbitMQPublisher publisher)
        {
            _repository = repository;
            _mapper = mapper;
            _publisher = publisher;
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
            var userEntity = _mapper.Map<User>(userUpdateDto);

            var updatedUser = await _repository.UpdateAsync(id, userEntity);

            if (updatedUser == null)
            {
                return null;
            }
            await _publisher.PublishUserUpsertedAsync(new UserUpsertedV1(updatedUser.Id, updatedUser.Email, updatedUser.FullName, updatedUser.PhoneNumber, updatedUser.AvatarUrl));
            return _mapper.Map<UserDTO>(updatedUser);
        }

        public async Task<UserDTO> CreateUser(UserCreateDTO userCreateDto)
        {
            var userEntity = _mapper.Map<User>(userCreateDto);

            var newUser = await _repository.CreateAsync(userEntity);
            await _publisher.PublishUserUpsertedAsync(new UserUpsertedV1(newUser.Id, newUser.Email, newUser.FullName, newUser.PhoneNumber, newUser.AvatarUrl));

            return _mapper.Map<UserDTO>(newUser);
        }
    }
}
