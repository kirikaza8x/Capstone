using Users.Domain.Enums;

namespace Users.Domain.Entities
{
    public partial class User
    {
        // --------------------
        // Profile
        // --------------------
        public string? FirstName { get; private set; }
        public string? LastName { get; private set; }
        public DateTime? Birthday { get; private set; }
        public Gender? Gender { get; private set; }
        public string? PhoneNumber { get; private set; }
        public string? Address { get; private set; }
        public string? Description { get; private set; }
        public string? SocialLink { get; private set; }
        public string? ProfileImageUrl { get; private set; }

        // --------------------
        // Wallet
        // --------------------
        // public Wallet? Wallet { get; private set; }

        public void UpdateProfileImage(string imageUrl)
            => ProfileImageUrl = imageUrl;

        public void UpdateProfile(
            string? firstName,
            string? lastName,
            DateTime? birthday,
            Gender? gender,
            string? phone,
            string? address,
            string? description,
            string? socialLink,
            string? profileImageUrl)
        {
            FirstName = firstName ?? FirstName;
            LastName = lastName ?? LastName;
            Birthday = birthday ?? Birthday;
            Gender = gender ?? Gender;
            PhoneNumber = phone ?? PhoneNumber;
            Address = address ?? Address;
            Description = description ?? Description;
            SocialLink = socialLink ?? SocialLink;
            ProfileImageUrl = profileImageUrl ?? ProfileImageUrl;
        }

        // public void AttachWallet(Wallet wallet)
        // {
        //     if (Wallet != null)
        //         throw new InvalidOperationException("User already has a wallet.");

        //     Wallet = wallet;
        // }
    }
}
