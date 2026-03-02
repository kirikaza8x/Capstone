using ClosedXML.Excel;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Report;

namespace Users.Infrastructure.ImportExport
{
    public class UserExcelMappings : ISheetMappings<User>
    {
        private readonly IPasswordHasher _passwordHasher;

        public UserExcelMappings(IPasswordHasher passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public Func<object, User> GetRowMapper()
        {
            return rowObj =>
            {
                var row = (IXLRow)rowObj;

                var email = row.Cell(1).GetString();
                var userName = row.Cell(2).GetString();
                var plainPass = row.Cell(3).GetString();
                var firstName = row.Cell(4).GetString();
                var lastName = row.Cell(5).GetString();
                var birthday = row.Cell(6).GetDateTime();
                var genderStr = row.Cell(7).GetString();
                var phoneNumber = row.Cell(8).GetString();
                var address = row.Cell(9).GetString();
                var description = row.Cell(10).GetString();
                var socialLink = row.Cell(11).GetString();
                var statusStr = row.Cell(12).GetString();
                var isActiveStr = row.Cell(13).GetString();

                Gender? gender = Enum.TryParse<Gender>(genderStr, true, out var g) ? g : null;
                UserStatus status = Enum.TryParse<UserStatus>(statusStr, true, out var s) ? s : UserStatus.Active;
                bool isActive = bool.TryParse(isActiveStr, out var active) ? active : true;

                var hashedPassword = _passwordHasher.HashPassword(
                    string.IsNullOrWhiteSpace(plainPass) ? "Default123!" : plainPass
                );

                var user = User.CreateSheet(
                    email,
                    userName,
                    hashedPassword,
                    firstName,
                    lastName,
                    phoneNumber,
                    address,
                    birthday,
                    gender,
                    description,
                    socialLink,
                    null,
                    status
                );

                user.IsActive = isActive;
                return user;
            };
        }

        public Action<object, IEnumerable<User>> Exporter => (wsObj, users) =>
        {
            var ws = (IXLWorksheet)wsObj;

            ws.Cell(1, 1).Value = "Email";
            ws.Cell(1, 2).Value = "UserName";
            ws.Cell(1, 3).Value = "Password";
            ws.Cell(1, 4).Value = "FirstName";
            ws.Cell(1, 5).Value = "LastName";
            ws.Cell(1, 6).Value = "Birthday";
            ws.Cell(1, 7).Value = "Gender";
            ws.Cell(1, 8).Value = "PhoneNumber";
            ws.Cell(1, 9).Value = "Address";
            ws.Cell(1, 10).Value = "Description";
            ws.Cell(1, 11).Value = "SocialLink";
            ws.Cell(1, 12).Value = "Status";
            ws.Cell(1, 13).Value = "IsActive";

            int row = 2;
            foreach (var user in users)
            {
                ws.Cell(row, 1).Value = user.Email;
                ws.Cell(row, 2).Value = user.UserName;
                ws.Cell(row, 3).Value = "[HASHED]";
                ws.Cell(row, 4).Value = user.FirstName;
                ws.Cell(row, 5).Value = user.LastName;
                ws.Cell(row, 6).Value = user.Birthday?.ToString("yyyy-MM-dd");
                ws.Cell(row, 7).Value = user.Gender?.ToString();
                ws.Cell(row, 8).Value = user.PhoneNumber;
                ws.Cell(row, 9).Value = user.Address;
                ws.Cell(row, 10).Value = user.Description;
                ws.Cell(row, 11).Value = user.SocialLink;
                ws.Cell(row, 12).Value = user.Status.ToString();
                ws.Cell(row, 13).Value = user.IsActive;
                row++;
            }
        };
    }
}
