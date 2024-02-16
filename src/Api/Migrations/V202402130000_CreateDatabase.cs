using Domain.Contracts;
using FluentMigrator;

namespace Web.Migrations;

[Migration(202402130000, "Creating the database")]
public class V202402130000_CreateDatabase : Migration
{
    public override void Up()
    {
        if (!Schema.Table("accounts").Exists())
        {
            Create.Table("accounts")
               .WithColumn("id").AsInt64().PrimaryKey().Identity()
               .WithColumn("login").AsString(32).Unique().NotNullable()
               .WithColumn("password_hash").AsString().Nullable()
               .WithColumn("role").AsInt32().NotNullable().WithDefaultValue(0)
               .WithColumn("balance").AsInt64().NotNullable()
               .WithColumn("reserved_amount").AsInt64().NotNullable()
               .WithColumn("is_blocked").AsBoolean().NotNullable()
               .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false);
        }

        if (!Schema.Table("lots").Exists())
        {
            Create.Table("lots")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("title").AsString(64).NotNullable()
                .WithColumn("description").AsString(4096).NotNullable()
                .WithColumn("min_price").AsInt64().NotNullable()
                .WithColumn("current_price").AsInt64().NotNullable()
                .WithColumn("created_at").AsDateTimeOffset().NotNullable()
                .WithColumn("opening_at").AsDateTimeOffset().NotNullable()
                .WithColumn("closing_at").AsDateTimeOffset().NotNullable()
                .WithColumn("seller_id").AsInt64().NotNullable().ForeignKey("accounts", "id")
                .WithColumn("winner_id").AsInt64().Nullable().ForeignKey("accounts", "id")
                .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false);
        }

        if (!Schema.Table("account_transactions").Exists())
        {
            Create.Table("account_transactions")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("recipient_id").AsInt64().NotNullable().ForeignKey("accounts", "id")
            .WithColumn("sender_id").AsInt64().Nullable().ForeignKey("accounts", "id")
            .WithColumn("type").AsInt32().NotNullable()
            .WithColumn("amount").AsInt64().NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable();
        }

        if (!Schema.Table("bids").Exists())
        {
            Create.Table("bids")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("lot_id").AsInt64().NotNullable().ForeignKey("lots", "id")
            .WithColumn("account_id").AsInt64().NotNullable().ForeignKey("accounts", "id")
            .WithColumn("transaction_id").AsInt64().Nullable().ForeignKey("account_transactions", "id")
            .WithColumn("price").AsInt64().NotNullable()
            .WithColumn("is_recalled").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTimeOffset().NotNullable();

            Create.UniqueConstraint().OnTable("bids").Column("transaction_id");
        }

        if (!Schema.Table("refresh_tokens").Exists())
        {
            Create.Table("refresh_tokens")
            .WithColumn("token").AsString().PrimaryKey().NotNullable()
            .WithColumn("account_id").AsInt64().NotNullable().ForeignKey("accounts", "id")
            .WithColumn("created_at").AsDateTimeOffset().NotNullable()
            .WithColumn("expired_at").AsDateTimeOffset().NotNullable()
            .WithColumn("is_revoked").AsBoolean().WithDefaultValue(false);
        }
    }

    public override void Down()
    {
        Delete.Table("sessions");
        Delete.Table("bids");
        Delete.Table("account_transactions");
        Delete.Table("lots");
        Delete.Table("accounts");
    }
}
