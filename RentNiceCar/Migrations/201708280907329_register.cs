namespace RentNiceCar.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class register : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Adres", c => c.String());
            AddColumn("dbo.AspNetUsers", "Woonplaats", c => c.String());
            AddColumn("dbo.AspNetUsers", "Postcode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Postcode");
            DropColumn("dbo.AspNetUsers", "Woonplaats");
            DropColumn("dbo.AspNetUsers", "Adres");
        }
    }
}
