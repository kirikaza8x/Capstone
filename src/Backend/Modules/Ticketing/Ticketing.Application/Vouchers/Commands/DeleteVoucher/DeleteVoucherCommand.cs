using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Vouchers.Commands.DeleteVoucher;

public sealed record DeleteVoucherCommand(Guid VoucherId) : ICommand;
