﻿//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace com.mirle.ibg3k0.sc
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class OHTC_DevEntities : DbContext
    {
        public OHTC_DevEntities()
            : base("name=OHTC_DevEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ABASEDATA_VER> ABASEDATA_VER { get; set; }
        public virtual DbSet<ABLOCKZONEDETAIL> ABLOCKZONEDETAIL { get; set; }
        public virtual DbSet<AGROUPRAILS> AGROUPRAILS { get; set; }
        public virtual DbSet<AMAIN_VER> AMAIN_VER { get; set; }
        public virtual DbSet<APOINT> APOINT { get; set; }
        public virtual DbSet<ARAIL> ARAIL { get; set; }
        public virtual DbSet<ASEGMENT> ASEGMENT { get; set; }
        public virtual DbSet<ASUB_VER> ASUB_VER { get; set; }
        public virtual DbSet<BLOCKZONEQUEUE> BLOCKZONEQUEUE { get; set; }
        public virtual DbSet<ABLOCKZONEMASTER> ABLOCKZONEMASTER { get; set; }
        public virtual DbSet<ASEQUENCE> ASEQUENCE { get; set; }
        public virtual DbSet<ACYCLEZONEDETAIL> ACYCLEZONEDETAIL { get; set; }
        public virtual DbSet<ACYCLEZONEMASTER> ACYCLEZONEMASTER { get; set; }
        public virtual DbSet<ACYCLEZONETYPE> ACYCLEZONETYPE { get; set; }
        public virtual DbSet<AADDRESS> AADDRESS { get; set; }
        public virtual DbSet<ACMD_OHTC> ACMD_OHTC { get; set; }
        public virtual DbSet<ACEID> ACEID { get; set; }
        public virtual DbSet<ARPTID> ARPTID { get; set; }
        public virtual DbSet<AVIDINFO> AVIDINFO { get; set; }
        public virtual DbSet<ANETWORKQUALITY> ANETWORKQUALITY { get; set; }
        public virtual DbSet<APORTICON> APORTICON { get; set; }
        public virtual DbSet<BCSTAT> BCSTAT { get; set; }
        public virtual DbSet<AEQPT> AEQPT { get; set; }
        public virtual DbSet<ALINE> ALINE { get; set; }
        public virtual DbSet<AZONE> AZONE { get; set; }
        public virtual DbSet<ANODE> ANODE { get; set; }
        public virtual DbSet<AUNIT> AUNIT { get; set; }
        public virtual DbSet<ABUFFER> ABUFFER { get; set; }
        public virtual DbSet<ACASSETTE> ACASSETTE { get; set; }
        public virtual DbSet<ACRATE> ACRATE { get; set; }
        public virtual DbSet<AECDATAMAP> AECDATAMAP { get; set; }
        public virtual DbSet<AEVENTRPTCOND> AEVENTRPTCOND { get; set; }
        public virtual DbSet<AFLOW_REL> AFLOW_REL { get; set; }
        public virtual DbSet<UASFNC> UASFNC { get; set; }
        public virtual DbSet<ALOT> ALOT { get; set; }
        public virtual DbSet<HOPERATION> HOPERATION { get; set; }
        public virtual DbSet<ASHEET> ASHEET { get; set; }
        public virtual DbSet<HASHEET> HASHEET { get; set; }
        public virtual DbSet<ATRACEITEM> ATRACEITEM { get; set; }
        public virtual DbSet<ATRACESET> ATRACESET { get; set; }
        public virtual DbSet<UASUSR> UASUSR { get; set; }
        public virtual DbSet<UASUSRGRP> UASUSRGRP { get; set; }
        public virtual DbSet<UASUFNC> UASUFNC { get; set; }
        public virtual DbSet<APORTSTATION> APORTSTATION { get; set; }
        public virtual DbSet<APORT> APORT { get; set; }
        public virtual DbSet<AMCSREPORTQUEUE> AMCSREPORTQUEUE { get; set; }
        public virtual DbSet<AADDRESS_DATA> AADDRESS_DATA { get; set; }
        public virtual DbSet<ACMD_OHTC_DETAIL> ACMD_OHTC_DETAIL { get; set; }
        public virtual DbSet<APARKZONEDETAIL> APARKZONEDETAIL { get; set; }
        public virtual DbSet<APARKZONEMASTER> APARKZONEMASTER { get; set; }
        public virtual DbSet<APARKZONETYPE> APARKZONETYPE { get; set; }
        public virtual DbSet<APORT_POSITION_TEACHING_DATA> APORT_POSITION_TEACHING_DATA { get; set; }
        public virtual DbSet<ASECTION> ASECTION { get; set; }
        public virtual DbSet<ASECTION_CONTROL_100> ASECTION_CONTROL_100 { get; set; }
        public virtual DbSet<AVEHICLE_CONTROL_100> AVEHICLE_CONTROL_100 { get; set; }
        public virtual DbSet<CONTROL_DATA> CONTROL_DATA { get; set; }
        public virtual DbSet<SCALE_BASE_DATA> SCALE_BASE_DATA { get; set; }
        public virtual DbSet<ASYSEXCUTEQUALITY> ASYSEXCUTEQUALITY { get; set; }
        public virtual DbSet<AVEHICLE> AVEHICLE { get; set; }
        public virtual DbSet<AHIDZONEDETAIL> AHIDZONEDETAIL { get; set; }
        public virtual DbSet<AHIDZONEMASTER> AHIDZONEMASTER { get; set; }
        public virtual DbSet<AHIDZONEQUEUE> AHIDZONEQUEUE { get; set; }
        public virtual DbSet<ALARMRPTCOND> ALARMRPTCOND { get; set; }
        public virtual DbSet<ALARM> ALARM { get; set; }
        public virtual DbSet<ALARMMAP> ALARMMAP { get; set; }
        public virtual DbSet<ZoneDef> ZoneDef { get; set; }
        public virtual DbSet<ACMD_MCS> ACMD_MCS { get; set; }
        public virtual DbSet<CassetteData> CassetteData { get; set; }
        public virtual DbSet<ShelfDef> ShelfDef { get; set; }
        public virtual DbSet<PortDef> PortDef { get; set; }
        public virtual DbSet<HCMD_MCS> HCMD_MCS { get; set; }
        public virtual DbSet<HCMD_OHTC> HCMD_OHTC { get; set; }
        public virtual DbSet<VSECTION_100> VSECTION_100 { get; set; }
        public virtual DbSet<HALARM> HALARM { get; set; }
        public virtual DbSet<VACMD_MCS> VACMD_MCS { get; set; }
        public virtual DbSet<VHCMD_OHTC_MCS> VHCMD_OHTC_MCS { get; set; }
    }
}
