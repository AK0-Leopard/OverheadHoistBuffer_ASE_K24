using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.sc.Data;
using CommonMessage.ProtocolFormat.ShelfFun;
using Grpc.Core;
namespace com.mirle.ibg3k0.sc.WebAPI.Grpc
{
    internal class Shelf : shelfGreeter.shelfGreeterBase
    {
        App.SCApplication app;
        public Shelf(App.SCApplication _app)
        {
            app = _app;
        }
        public override Task<replyAllShelfInfo> getAllShelfInfo(Empty empty, ServerCallContext context)
        {
            replyAllShelfInfo result = new replyAllShelfInfo();
            //var allShelfData = app.ShelfDefDao.LoadShelfDef(DBConnection_EF.GetUContext());
            var allShelfData = app.ShelfDefBLL.LoadShelf();
            foreach(var shelfData in allShelfData)
            {
                shelf temp = new shelf();
                temp.BoxId = shelfData.CSTID;
                temp.Enable = (shelfData.Enable=="Enable") ? true : false;
                temp.ShelfId = shelfData.ShelfID;
                temp.ZoneId = shelfData.ZoneID;
                temp.AdrId = shelfData.ADR_ID;
                result.ShelfInfo.Add(temp);
            }
            return Task.FromResult(result);
        }
        public override Task<shelf> getShelfInfo(shelf_id id, ServerCallContext context)
        {
            ShelfDef shelfDef;
            shelf result = new shelf();
            try
            {
                shelfDef = app.ShelfDefBLL.LoadShelf().Find(shelf => shelf.ShelfID == id.ID);
                result.BoxId = shelfDef.CSTID;
                result.Enable = (shelfDef.Enable == "Enable");
                result.ShelfId = shelfDef.ShelfID;
                result.ZoneId = shelfDef.ZoneID;
                result.AdrId = shelfDef.ADR_ID;
                
            }
            catch (Exception ex)
            {
                //如果find找不到就會跳例外所以在這邊處理
                result.BoxId = "";
                result.Enable = false;
                result.ShelfId = "";
                result.ZoneId = "";
                result.AdrId = "";
            }
            return Task.FromResult(result); //不管find有找到或找不到跳例外都會回傳
        }
        public override Task<replyAllShelfInfo> getNeedChangeShelf(lastUpdateTime dataTime, ServerCallContext context)
        {
            replyAllShelfInfo result = new replyAllShelfInfo();
            //var allShelfData = app.ShelfDefDao.LoadShelfDef(DBConnection_EF.GetUContext());
            var allShelfData = app.ShelfDefBLL.LoadShelf();
            DateTime userLastUpdateTime = DateTime.FromBinary(dataTime.Datetime);//這邊是client告訴我們他最後更新的時間
            DateTime dataLastUpdateTime;
            foreach (var shelfData in allShelfData)
            {
                dataLastUpdateTime = DateTime.FromBinary(Convert.ToInt64(shelfData.TrnDT));
                if(userLastUpdateTime > dataLastUpdateTime)
                {
                    shelf temp = new shelf();
                    temp.BoxId = shelfData.CSTID;
                    temp.Enable = (shelfData.Enable == "Enable") ? true : false;
                    temp.ShelfId = shelfData.ShelfID;
                    temp.ZoneId = shelfData.ZoneID;
                    temp.AdrId = shelfData.ADR_ID;
                    result.ShelfInfo.Add(temp);
                }
            }
            return Task.FromResult(result);
        }
    }
}
