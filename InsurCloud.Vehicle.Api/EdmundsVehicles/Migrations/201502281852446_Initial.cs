namespace EdmundsVehicles.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "vehicles.Vehicles",
                c => new
                    {
                        StyleId = c.String(nullable: false, maxLength: 128),
                        StyleName = c.String(),
                        Trim = c.String(),
                        SubModelBodyType = c.String(),
                        SubModelName = c.String(),
                        SubModelNiceName = c.String(),
                        ModelYear = c.Int(nullable: false),
                        MakeId = c.String(),
                        MakeName = c.String(),
                        MakeNiceName = c.String(),
                        ModelId = c.String(),
                        ModelName = c.String(),
                        ModelNiceName = c.String(),
                    })
                .PrimaryKey(t => t.StyleId);
            
        }
        
        public override void Down()
        {
            DropTable("vehicles.Vehicles");
        }
    }
}
