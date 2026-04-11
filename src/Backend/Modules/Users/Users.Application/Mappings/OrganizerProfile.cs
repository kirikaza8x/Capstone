using AutoMapper;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Entities;
using Users.Domain.Enums;

public class OrganizersProfile : Profile
{
    public OrganizersProfile()
    {
        // ========================
        // ADMIN LIST
        // ========================
        CreateMap<OrganizerProfile, OrganizerAdminListItemDto>()
            .ForMember(dest => dest.ProfileId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => src.Logo));

        // ========================
        // FULL RESPONSE (ADMIN DETAIL)
        // ========================
        CreateMap<OrganizerProfile, OrganizerProfileResponseDto>()
        .ForMember(d => d.BusinessType, o => o.MapFrom(s => s.BusinessType.ToString()))
        .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
        .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
        .ForMember(d => d.TaxCode, o => o.MapFrom(s => s.TaxCode))
        .ForMember(d => d.IdentityNumber, o => o.MapFrom(s => s.IdentityNumber))
        .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.CompanyName))
        .ForMember(d => d.AccountName, o => o.MapFrom(s => s.AccountName))
        .ForMember(d => d.AccountNumber, o => o.MapFrom(s => s.AccountNumber))
        .ForMember(d => d.BankCode, o => o.MapFrom(s => s.BankCode))
        .ForMember(d => d.Branch, o => o.MapFrom(s => s.Branch))
        .ForMember(d => d.VerifiedAt, o => o.MapFrom(s => s.VerifiedAt));


        // ========================
        // ORGANIZER SELF VIEW
        // ========================
        CreateMap<OrganizerProfile, MyOrganizerProfileDto>()
            .ForMember(d => d.ProfileId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.CanEdit, o => o.MapFrom(s =>
                s.Status == OrganizerStatus.Draft ||
                s.Status == OrganizerStatus.Rejected))
            .ForMember(d => d.CanSubmit, o => o.MapFrom(s =>
                s.Status == OrganizerStatus.Draft));

        // ========================
        // PUBLIC VIEW
        // ========================
        CreateMap<OrganizerProfile, OrganizerPublicProfileDto>();
    }
}