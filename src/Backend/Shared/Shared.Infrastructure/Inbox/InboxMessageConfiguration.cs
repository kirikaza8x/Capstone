//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using System;
//using System.Collections.Generic;
//namespace Shared.Infrastructure.Inbox;

//internal sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
//{
//    public void Configure(EntityTypeBuilder<InboxMessage> builder)
//    {
//        builder.ToTable("inbox_messages");

//        builder.HasKey(x => x.Id);

//        builder.Property(x => x.Type)
//            .HasMaxLength(500)
//            .IsRequired();

//        builder.Property(x => x.Content)
//            .HasColumnType("jsonb")
//            .IsRequired();

//        builder.Property(x => x.OccurredOnUtc)
//            .IsRequired();

//        builder.Property(x => x.ProcessedOnUtc);

//        builder.Property(x => x.Error)
//            .HasMaxLength(2000);

//        builder.HasIndex(x => x.ProcessedOnUtc);
//        builder.HasIndex(x => x.Error);
//    }
//}
