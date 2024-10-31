using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Pig.AspNetCore.Application.DTOs;
using Pig.AspNetCore.Application.Mapping;

namespace LOYALTY.QuanTest.DTO
{

    public class UserTestDTO : IMapFrom<UserTest>
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public string Email { get; set; }

        public void Mapping(Profile profile)
        {
        }
    }
}
