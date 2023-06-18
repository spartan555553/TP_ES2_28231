using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessLogic.Context;
using BusinessLogic.Entities;

//Store the information of the Logged in User
public static class UserSession
{
    private static User _loggedInUser;

    public static User LoggedInUser
    {
        get { return _loggedInUser; }
        set { _loggedInUser = value; }
    }
    
    public static void Logout()
    {
        LoggedInUser = null;
    }

    public static int GetLoggedInUserId()
    {
        // Replace this code with your actual logic to query the database and retrieve the user ID based on the username and password
        using (var dbContext = new ES2DbContext())
        {
            var user = dbContext.Users.SingleOrDefault(u => u.Username == LoggedInUser.Username && u.Password == LoggedInUser.Password);
            return user?.UserId ?? 0; // Return the user ID if found, or 0 if not found
        }
    }
    
    public static string GetLoggedInUserType(int loggedInUserId)
    {
        // Replace this code with your actual logic to query the database and retrieve the UserType based on the user ID
        using (var dbContext = new ES2DbContext())
        {
            var user = dbContext.Users.SingleOrDefault(u => u.UserId == loggedInUserId);
            return user?.UserType;
        }
    }
    
    public static string GetLoggedInUserStatus(int loggedInUserId)
    {
        // Replace this code with your actual logic to query the database and retrieve the UserType based on the user ID
        using (var dbContext = new ES2DbContext())
        {
            var user = dbContext.Users.SingleOrDefault(u => u.UserId == loggedInUserId);
            return user?.UserStatus;
        }
    }

    
    public static int GetLoggedId()
    {
        return _loggedInUser?.UserId ?? 0;
    }

}

namespace Backend.Controllers
{
    [Route("api/Users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ES2DbContext _context;

        public UsersController(ES2DbContext context)
        {
            _context = context;
        }

        //Method to Login
        [HttpPost("login")]
        public async Task<IActionResult> HandleLogin(string username, string password)
        {
            // Check if the username and password match the records in the database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                // Authentication successful
                UserSession.LoggedInUser = user;
                return Ok("Login successful!");
            }
            else
            {
                // Authentication failed
                return Unauthorized("Invalid username or password");
            }
        }
        
        //Method to Logout
        [HttpPost("logout")]
        public IActionResult HandleLogout()
        {
            UserSession.Logout();
            return Ok("Logout successful!");
        }

        //Method to Create a new User
        [HttpPost("create")]
        public async Task<IActionResult> HandleCreateUser([FromQuery] string username, [FromQuery] string password)
        {
            // Set default values for user_type and user_status
            var user = new User
            {
                Username = username,
                Password = password,
                UserType = "User",
                UserStatus = "Active"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User created successfully!");
        }
        
        //Method that returns all Users
        [HttpGet("get")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();

                var usersDTOs = users.Select(u => new
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Password = u.Password,
                    UserType = u.UserType,
                    UserStatus = u.UserStatus
                });

                return Ok(usersDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        
        [HttpGet("get/{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user != null)
                {
                    var userDTO = new
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Password = user.Password,
                        UserType = user.UserType,
                        UserStatus = user.UserStatus
                    };

                    return Ok(userDTO);
                }
                else
                {
                    return NotFound("No user found with the specified ID.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        //Method that list Users Informations
        [HttpGet("list")]
        public async Task<IActionResult> ListUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();

                var usersDTOs = users.Select(u => new
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Password = u.Password,
                    UserType = u.UserType,
                    UserStatus = u.UserStatus
                });

                return Ok(usersDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        
        //Method to search Users by name and type
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers(string searchTerm)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.Username.Contains(searchTerm) || u.UserType.Contains(searchTerm))
                    .ToListAsync();

                var usersDTOs = users.Select(u => new
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Password = u.Password,
                    UserType = u.UserType,
                    UserStatus = u.UserStatus
                });

                return Ok(usersDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        
        //Method to change the UserStatus of a User
        [HttpDelete("disable/{userId}")]
        public async Task<IActionResult> DisableSelectedUser(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user != null)
                {
                    if (user.UserStatus == "Ativo")
                    {
                        user.UserStatus = "Inativo";
                    }
                    else
                    {
                        user.UserStatus = "Ativo";
                    }
                    
                    await _context.SaveChangesAsync();

                    return Ok("User status updated successfully.");
                }
                else
                {
                    return NotFound("No user found with the specified ID.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        
        [HttpPut("update/{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] User updatedUser)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user != null)
                {
                    // Update the user attributes based on the provided updatedUser object
                    user.Username = updatedUser.Username;
                    user.Password = updatedUser.Password;
                    user.UserType = updatedUser.UserType;
                    user.UserStatus = updatedUser.UserStatus;

                    await _context.SaveChangesAsync();

                    return Ok("User updated successfully.");
                }
                else
                {
                    return NotFound("No user found with the specified ID.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}
