using FluentMigrator;

namespace Web.Migrations;

[Migration(202402130000, "Creating the database")]
public class V202402130000_CreateDatabase : Migration
{
    public override void Up()
    {

        Create.Table("accounts")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("login").AsString().Unique().NotNullable()
            .WithColumn("password_hash").AsString().Nullable()
            .WithColumn("role_id").AsInt32().NotNullable().ForeignKey("roles", "id")
            .WithColumn("balance").AsInt64().NotNullable()
            .WithColumn("reserved_amount").AsInt64().NotNullable()
            .WithColumn("is_blocked").AsBoolean().NotNullable()
            .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false);

        Create.Table("lots")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("title").AsString(96).NotNullable()
            .WithColumn("description").AsString(4096).NotNullable()
            .WithColumn("min_price").AsInt64().NotNullable()
            .WithColumn("current_price").AsInt64().NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable()
            .WithColumn("opening_at").AsDateTimeOffset().NotNullable()
            .WithColumn("closing_at").AsDateTimeOffset().NotNullable()
            .WithColumn("seller_id").AsInt64().NotNullable().ForeignKey("accounts", "id")
            .WithColumn("winner_id").AsInt64().Nullable().ForeignKey("accounts", "id")
            .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false);

        Create.Table("account_transactions")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("account_id").AsInt64().NotNullable().ForeignKey("accounts", "id")
            .WithColumn("type").AsInt32().NotNullable()
            .WithColumn("amount").AsInt64().NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable();

        Create.Table("bids")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("lot_id").AsInt64().NotNullable().ForeignKey("lots", "id")
            .WithColumn("account_id").AsInt64().NotNullable().ForeignKey("accounts", "id")
            .WithColumn("transaction_id").AsInt64().NotNullable().ForeignKey("account_transactions", "id")
            .WithColumn("price").AsInt64().NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable();

        Create.UniqueConstraint().OnTable("bids").Column("transaction_id");

        Create.Table("refresh_tokens")
            .WithColumn("token").AsString().PrimaryKey().NotNullable()
            .WithColumn("account_id").AsInt64().NotNullable().ForeignKey("accounts", "id")
            .WithColumn("created_at").AsDateTimeOffset().NotNullable()
            .WithColumn("expired_at").AsDateTimeOffset().NotNullable()
            .WithColumn("is_revoked").AsBoolean().WithDefaultValue(false);
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
