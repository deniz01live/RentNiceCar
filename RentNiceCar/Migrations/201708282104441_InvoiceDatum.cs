namespace RentNiceCar.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InvoiceDatum : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Invoices", "Datum", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AddColumn("dbo.Invoices", "DateAt", c => c.DateTime(nullable: false));
        }
    }
}
