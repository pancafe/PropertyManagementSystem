using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Dynamic;
using System.IO;
using System.Text;
using PMS.Models;
using PMS.filter;

namespace PMS.Controllers
{
    [MyCheckFilterAttribute(IsCheck=true)]
    public class OwnerController : Controller
    {
        private PMS_EF db = new PMS_EF();

        // GET: /Owner/
        public ActionResult Index()
        {

            Session["loginname"] = Request.Params["displayname"];

            string login = Request.Params["displayname"];
            var oid = db.owner.Where(b => b.Oname == login).Select(b => b.Oid).FirstOrDefault();

            var num = db.tenant.Where(b => b.Oid == oid).Count();
            ViewBag.counts = num;

            //新增
            var notice = db.propertycompany.Where(u=>u.Pnotice!=null & u.Prules!=null);
            List<dynamic> oneList = new List<dynamic>();
            foreach (var item in notice.ToList())
            {
                dynamic dyObj = new ExpandoObject();
                dyObj.Pnotice = item.Pnotice;
                dyObj.Prules = item.Prules;
                oneList.Add(dyObj);
            }
            ViewBag.data = oneList;

            var owner = db.owner.Include(o => o.propertycompany);
            return View(owner.ToList());
        }
        public ActionResult logout()
        {
              //return RedirectToAction("Index", "Home");
            //清空全部session缓存
            Session.Abandon();
            return Content("<script>window.location.href='/Home/Index';</script>");
        }

        //确认上传按钮
       
        [HttpPost]
        public ActionResult UploadImage()
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase f = Request.Files["ownerfile"];
                if (f.FileName == "")
                {
                    return Content("<script>alert('请先选择文件');window.location.href='/Owner/bill_search_del_edit';</script>");
                }
                else
                {
                    string type = f.FileName.Substring(f.FileName.IndexOf("."));
                    if (type == ".jpg" || type == ".gif" || type == ".png" || type == ".bmg" || type == ".jpeg")
                    {
                        f.SaveAs(@"D:\C#\PMS\PMS\Uploaded\" + oid + type);

                        var owner = db.owner.FirstOrDefault(u => u.Oid == oid);
                        if (owner != null)
                        {
                            owner.Oqrcode = "/Uploaded/"+oid+type;
                        }
                        db.SaveChanges();
                        return Content("<script>alert('上传成功！');window.location.href='/Owner/bill_search_del_edit';</script>");
                    }
                    else
                    {
                        return Content("<script>alert('图片格式不正确！');window.location.href='/Owner/bill_search_del_edit';</script>");
                    }
                }
                
                
            }
            else
            {
                return Content("<script>alert('上传失败！');window.location.href='/Owner/bill_search_del_edit';</script>");
            }
            
        }




        //业主申请服务
        [HttpPost]
        public ActionResult serviceapplication(FormCollection fc)
        {
            if (fc["service"] == "" || fc["oname"] == ""|| fc["ocontact"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {
                string service = fc["service"];
                string oname = fc["oname"];
                string ocontact = fc["ocontact"];

                DateTime time = DateTime.Now;
 
               

                string loginname = Session["loginname"].ToString();
                var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

                var newapplication = new application
                {
                    Oid=oid,
                    Oname=oname,
                    Ocontact=ocontact,
                    Atime=time,
                    Astatus="未受理",
                    Adetail=service
                };

                db.application.Add(newapplication);

                db.SaveChanges();
                return Content("<script>alert('添加成功！');window.location.href='/Owner/applicaion_search_del_edit';</script>");

            }
        }

        //撤销删除申请
        public ActionResult applicationdelete(int? id)
        {
           application application = db.application.Find(id);
           db.application.Remove(application);
           db.SaveChanges();
           return Content("<script>alert('撤销成功！');window.location.href='/Owner/applicaion_search_del_edit';</script>");
        }

        //业主申请服务_按日期查询
        [HttpPost]
        public ActionResult SearchByDate(FormCollection fc)
        {
            string d1 = fc["date1"];
            string d2 = fc["date2"];

            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            if (d1 == "" || d2 == "")
            {
                return Content("<script>alert('请输入日期后以进行查询');history.go(-1);</script>");
            }
            else
            {
                DateTime dt1 = Convert.ToDateTime(d1);
                DateTime dt2 = Convert.ToDateTime(d2);

                var com = db.application.Where(b => b.Atime >= dt1 && b.Atime <= dt2 &&b.Oid==oid);
                if (com.Count() > 0)
                {
                    return View("applicaion_search_del_edit", com);
                }
                else
                {
                    return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                }
            }

        }

        //业主投诉
        [HttpPost]
        public ActionResult suggestions(FormCollection fc)
        {
            if (fc["oname"] == "" || fc["suggestion"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {
                string oname = fc["oname"];
                string sugg = fc["suggestion"];

                string loginname = Session["loginname"].ToString();
                var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

                var owner = db.owner.FirstOrDefault(u => u.Oid == oid);

                    if (owner != null)
                    {
                        owner.Osuggestion = sugg;

                    }
                    db.SaveChanges();
                    return Content("<script>alert('提交成功！');history.go(-1);</script>");

                
            }
        }

        //新增租户账单
        [HttpPost]
        public ActionResult billAdd(FormCollection fc)
        {
            
            if (fc["tid"] == "" || fc["tname"] == "" || fc["usage"] == "" || fc["price"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {

                int tid = Convert.ToInt32(fc["tid"]);
                string tname = fc["tname"];
                string fee = fc["fee"];
                string usage = fc["usage"];
                string price = fc["price"];
                DateTime time = DateTime.Now;

                double usage1 = Convert.ToDouble(usage);
                double price1 = Convert.ToDouble(price);


                string loginname = Session["loginname"].ToString();
                var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

                var newbill = new bill
                {
                    Tid=tid,
                    Tname=tname,
                    Fname=fee,
                    Busage = usage1,
                    Bprice = price1,
                    Btime=time,
                    Oid=oid,
                    Bpaid=0,
                    Bunpaid=price1
                };

                ViewBag.pagestatus = "当前显示所有租户的房租账单";
                db.bill.Add(newbill);
                db.SaveChanges();
                return Content("<script>alert('添加成功！');window.location.href='/Owner/bill_search_del_edit';</script>");

            }
        }

        //修改账单信息
        [HttpPost]
        public ActionResult bill_edit(FormCollection fc)
        {
             if (fc["tid"] == "" || fc["tname"] == "" || fc["usage"] == "" || fc["price"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {

                int tid = Convert.ToInt32(fc["tid"]);
                string tname = fc["tname"];
                string fee = fc["fee"];
                double usage = Convert.ToDouble(fc["usage"]);
                double price = Convert.ToDouble(fc["price"]);
                DateTime time = DateTime.Now;

                

                string loginname = Session["loginname"].ToString();
                var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

                double usage1 = Convert.ToDouble(usage);
                double price1 = Convert.ToDouble(price);
                ViewBag.pagestatus = "当前显示所有租户的房租账单";

                var bill = db.bill.FirstOrDefault(u => u.Tid == tid );
                if (bill != null)
                {
                    bill.Tid = tid;
                    bill.Tname = tname;
                    bill.Fname = fee;
                    bill.Busage = usage;
                    bill.Bprice = price;
                    bill.Btime = time;
                    bill.Oid = oid;

                    db.SaveChanges();
                    return Content("<script>alert('修改成功！');window.location.href='/Owner/bill_search_del_edit';</script>");
                }
                else
                {
                    return Content("<script>alert('查无此账单记录');window.location.href='/Owner/bill_search_del_edit';</script>");
                }
                

            }
        }

        //删除账单记录
        public ActionResult billdelete(int? id,string name)
        {
            var bill = db.bill.FirstOrDefault(u => u.Tid == id && u.Fname == name);
            db.bill.Remove(bill);
            db.SaveChanges();
            ViewBag.pagestatus = "当前显示所有租户的房租账单";
            return Content("<script>alert('删除成功！');window.location.href='/Owner/bill_search_del_edit';</script>");
        }


        //催缴短信发送
        public ActionResult billnotice(int? id,string name)
        {
            //获取目标电话号码smsMob
            var tel= db.tenant.Where(b => b.Tid == id).Select(b => b.Tcontact).FirstOrDefault();
            string smsMob = Convert.ToString(tel);

            string THE_UID = "weirouto";//网建用户名
            string THE_KEY = "d41d8cd98f00b204e980";//接口密钥

         
            var com = db.bill.Where(t => t.Tid == id && t.Bunpaid!=0);
            string smsText = "尊敬的阳光小区租户，您本月未缴项目如下，请及时缴纳以防止对应服务终止造成不便:";//短信内容
          
            foreach (var item in com.ToList())
            {
                smsText = smsText + "[" + item.Fname + ":" + item.Bunpaid + "]";
            }


            string postUrl = "http://utf8.sms.webchinese.cn/?Uid=" + THE_UID + "&key=" + THE_KEY + "&smsMob=" + smsMob + "&smsText=" + smsText; 

            //发送短信
            try
            {
                HttpWebRequest hr = (HttpWebRequest)WebRequest.Create(postUrl);
                hr.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
                hr.Method = "GET";
                hr.Timeout = 30 * 60 * 1000;
                WebResponse hs = hr.GetResponse();
                Stream sr = hs.GetResponseStream();
                StreamReader ser = new StreamReader(sr, Encoding.Default);
                
            }
            catch (Exception ex)
            {
                
                return Content("<script>alert('出现未知的错误>_<，如下:'"+ex.Message+");history.go(-1);</script>");
               
            }

            return Content("<script>alert('通知（包含该租户全部未缴项目，不必重复发送）已发送!');history.go(-1);</script>");
            

        }

        //组合查询
        [HttpPost]
        public ActionResult CombineSearch(FormCollection fc)
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            if (fc["pay"] == "" & fc["date1"] == "" & fc["date2"] == "" & fc["keyword"] == "")
            {
                return Content("<script>alert('请至少选择一项进行查询');history.go(-1);</script>");
            }
            else
            {
                if (fc["pay"] != "" & fc["date1"] == "" & fc["date2"] == "" & fc["keyword"] == "")//已缴/未缴
                {
                    if (fc["pay"] == "未缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid != 0 && b.Fname == "房租" &&b.Oid==oid);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示未缴房租账单";

                        return View("bill_search_del_edit", com);
                    }
                    else if (fc["pay"] == "已缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid == 0 && b.Fname == "房租" && b.Oid == oid);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示已缴房租账单";


                        return View("bill_search_del_edit", com);
                    }
                    else
                    {
                        return Content("<script>alert('未知错误>_<');history.go(-1);</script>");
                    }
                }
                else if (fc["pay"] != "" & fc["date1"] != "" & fc["date2"] != "" & fc["keyword"] == "")//已缴未缴+日期
                {
                    string d1 = fc["date1"];
                    string d2 = fc["date2"];

                    DateTime dt1 = Convert.ToDateTime(d1);
                    DateTime dt2 = Convert.ToDateTime(d2);

                    if (fc["pay"] == "未缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid != 0 && b.Btime >= dt1 && b.Btime <= dt2 && b.Fname == "房租" && b.Oid == oid);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示在" + d1 + "-" + d2 + "之间未缴的房租账单";

                        return View("bill_search_del_edit", com);
                    }
                    else if (fc["pay"] == "已缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid == 0 && b.Btime >= dt1 && b.Btime <= dt2 && b.Fname == "房租" && b.Oid == oid);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示在" + d1 + "-" + d2 + "之间已缴的房租账单";

                        return View("bill_search_del_edit", com);
                    }
                    else
                    {
                        return Content("<script>alert('未知错误>_<');history.go(-1);</script>");
                    }

                }
                else if (fc["pay"] != "" & fc["date1"] == "" & fc["date2"] == "" & fc["keyword"] != "")//已缴未缴+租户编码
                {
                    string keyword = fc["keyword"];
                    int no = Convert.ToInt32(keyword);

                    if (fc["pay"] == "未缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid != 0 && b.Tid == no && b.Fname == "房租" && b.Oid == oid);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示租户" + no + "未缴的房租账单";

                        return View("bill_search_del_edit", com);
                    }
                    else if (fc["pay"] == "已缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid == 0 && b.Tid == no && b.Fname == "房租" && b.Oid == oid);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示租户" + no + "已缴的房租账单";

                        return View("bill_search_del_edit", com);
                    }
                    else
                    {
                        return Content("<script>alert('未知错误>_<');history.go(-1);</script>");
                    }

                }
                else if (fc["pay"] != "" & fc["date1"] != "" & fc["date2"] != "" & fc["keyword"] != "")//已缴未缴+日期+租户编码
                {
                    string d1 = fc["date1"];
                    string d2 = fc["date2"];

                    DateTime dt1 = Convert.ToDateTime(d1);
                    DateTime dt2 = Convert.ToDateTime(d2);

                    string keyword = fc["keyword"];
                    int no = Convert.ToInt32(keyword);

                    if (fc["pay"] == "未缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid != 0 && b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no && b.Fname == "房租" && b.Oid == oid);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示在" + d1 + "-" + d2 + "之间租户" + no + "未缴的房租账单";

                        return View("bill_search_del_edit", com);
                    }
                    else if (fc["pay"] == "已缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid == 0 && b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no && b.Fname == "房租" && b.Oid == oid);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示在" + d1 + "-" + d2 + "之间租户" + no + "已缴的房租账单";

                        return View("bill_search_del_edit", com);
                    }
                    else
                    {
                        return Content("<script>alert('未知错误>_<');history.go(-1);</script>");
                    }

                }
                else if (fc["pay"] == "" & fc["date1"] != "" & fc["date2"] != "" & fc["keyword"] == "")//日期
                {
                    string d1 = fc["date1"];
                    string d2 = fc["date2"];

                    DateTime dt1 = Convert.ToDateTime(d1);
                    DateTime dt2 = Convert.ToDateTime(d2);

                    var com = db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Fname == "房租" && b.Oid == oid);
                    if (com.Count() > 0)
                    {
                        //计算该日期区间的缴纳情况
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示在" + d1 + "-" + d2 + "之间的房租账单";

                        return View("bill_search_del_edit", com);
                    }
                    else
                    {
                        return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                    }

                }
                else if (fc["pay"] == "" & fc["date1"] != "" & fc["date2"] != "" & fc["keyword"] != "")//日期+编码
                {
                    string d1 = fc["date1"];
                    string d2 = fc["date2"];

                    DateTime dt1 = Convert.ToDateTime(d1);
                    DateTime dt2 = Convert.ToDateTime(d2);

                    string keyword = fc["keyword"];
                    int no = Convert.ToInt32(keyword);

                    var com = db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no && b.Fname == "房租" && b.Oid == oid);
                    if (com.Count() > 0)
                    {
                        //计算该日期区间的缴纳情况
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示在" + d1 + "-" + d2 + "之间租户" + no + "的房租账单";

                        return View("bill_search_del_edit", com);
                    }
                    else
                    {
                        return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                    }

                }
                else if (fc["pay"] == "" & fc["date1"] == "" & fc["date2"] == "" & fc["keyword"] != "")//编码
                {
                    string keyword = fc["keyword"];

                    int no = Convert.ToInt32(keyword);

                    var com = db.bill.Where(b => b.Tid == no && b.Fname == "房租" && b.Oid == oid);

                    if (com.Count() > 0)
                    {
                        //计算该租户的缴纳情况
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no && b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示租户" + no + "的房租账单";

                        return View("bill_search_del_edit", com);
                    }
                    else
                    {
                        return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                    }

                }
                else
                {
                    return Content("<script>alert('未知错误>_<');history.go(-1);</script>");
                }
            }

        }

        //清除上月房租账单
        public ActionResult deletelastmonth()
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();
             //获取当前系统时间 设置为 年-月-01
            string time = DateTime.Now.Year.ToString() +"-"+ DateTime.Now.Month.ToString()+"-01";
            DateTime dt = Convert.ToDateTime(time);
            //筛选在此日期之前并且已缴纳的账单
            db.bill.Where(d => d.Btime < dt && d.Bunpaid == 0 && d.Fname == "房租" && d.Oid == oid).ToList().ForEach(d => db.bill.Remove(d));
            db.SaveChanges();
           

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(b => b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(b => b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Fname == "房租" && b.Oid == oid).Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示全部房租账单";

            return Content("<script>alert('已删除" + time + "之前已缴的账单');window.location.href='/Owner/bill_search_del_edit';</script>");
        }

        //未缴已缴查看
        public ActionResult unpaidpage()
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            var com = db.bill.Where(b=>b.Oid==oid && b.Bunpaid!=0 && b.Fname=="房租");

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname=="房租").Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            return View("bill_search_del_edit", com);
        }
        public ActionResult paidpage()
        {
             string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            var com = db.bill.Where(b => b.Oid == oid && b.Bunpaid == 0 && b.Fname == "房租");

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;


            return View("bill_search_del_edit", com);
        }


        //搜索记录_根据租户编码
        [HttpPost]
        public ActionResult Search(FormCollection fc)
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            string keyword = fc["keyword"];


            if (keyword == "")
            {
                return Content("<script>alert('请输入编码以进行查询');history.go(-1);</script>");
            }
            else
            {
                int no = Convert.ToInt32(keyword);

                var com = db.bill.Where(b => b.Tid == no && b.Oid==oid && b.Fname=="房租");

                if (com.Count() > 0)
                {
                    //计算该租户的房租缴纳情况
                    double sum = Convert.ToDouble(db.bill.Where(b =>  b.Tid == no && b.Fname=="房租").Sum(s => s.Bprice));
                    double accepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no && b.Fname == "房租").Sum(s => s.Bpaid));
                    double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no && b.Fname == "房租").Sum(s => s.Bunpaid));
                    ViewBag.sum = sum;
                    ViewBag.accepted = accepted;
                    ViewBag.unaccepted = unaccepted;

                    return View("bill_search_del_edit", com);
                }
                else
                {
                    return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                }


            }
        }

        //查询账单记录_日期查询
        [HttpPost]
        public ActionResult SearchByDate1(FormCollection fc)
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            string d1 = fc["date1"];
            string d2 = fc["date2"];

            if (d1 == "" || d2 == "")
            {
                return Content("<script>alert('请输入日期后以进行查询');history.go(-1);</script>");
            }
            else
            {
                DateTime dt1 = Convert.ToDateTime(d1);
                DateTime dt2 = Convert.ToDateTime(d2);

                var com = db.bill.Where(b => b.Btime > dt1 && b.Btime <dt2 && b.Oid==oid && b.Fname=="房租");
                if (com.Count() > 0)
                {
                    //计算该区间的缴纳情况
                    double sum = Convert.ToDouble(db.bill.Where(b => b.Btime > dt1 && b.Btime < dt2 && b.Fname == "房租").Sum(s => s.Bprice));
                    double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime > dt1 && b.Btime < dt2 && b.Fname == "房租").Sum(s => s.Bpaid));
                    double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime > dt1 && b.Btime < dt2 && b.Fname == "房租").Sum(s => s.Bunpaid));
                    ViewBag.sum = sum;
                    ViewBag.accepted = accepted;
                    ViewBag.unaccepted = unaccepted;


                    return View("bill_search_del_edit", com);
                }
                else
                {
                    return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                }
            }
        }

        //合同添加
        [HttpPost]
        public ActionResult contractAdd(FormCollection fc)
        {
            if (fc["tid"] == "" || fc["tname"] == "" || fc["hid"]=="" || fc["cbegintime"] == "" || fc["cendtime"] == "" || fc["cpay"] == "" || fc["caccepted"] == "" || fc["cunaccepted"] == "" || fc["carea"] == "" || fc["cvalid"] == "" || fc["cinvalid"] == "" || fc["cstatus"] == "" || fc["cdate"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {

                int tid = Convert.ToInt32(fc["tid"]);
                string tname = fc["tname"];
                int hid = Convert.ToInt32(fc["hid"]);
                DateTime cbegintime =Convert.ToDateTime( fc["cbegintime"]);
                DateTime cendtime = Convert.ToDateTime(fc["cendtime"]);
                double cpay = Convert.ToDouble(fc["cpay"]);
                double caccepted = Convert.ToDouble(fc["caccepted"]);
                double cunaccepted = Convert.ToDouble(fc["cunaccepted"]);
                double carea = Convert.ToDouble(fc["carea"]);
                DateTime cvalid = Convert.ToDateTime(fc["cvalid"]);
                DateTime cinvalid = Convert.ToDateTime(fc["cinvalid"]);
                string cstatus = fc["cstatus"];
                DateTime cdate =Convert.ToDateTime( fc["cdate"]);

                string loginname = Session["loginname"].ToString();
                var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

                var exist = db.contract.FirstOrDefault(b => b.Tid == tid);
                if (exist != null)
                {
                    return Content("<script>alert('该租户已存在，请重新输入');history.go(-1);</script>");
                }
                else
                {
                    var newcontract = new contract
                    {
                        Tid = tid,
                        Tname = tname,
                        Hid = hid,
                        Cbegintime = cbegintime,
                        Cendtime = cendtime,
                        Cpay = cpay,
                        Caccepted = caccepted,
                        Cunaccepted = cunaccepted,
                        Carea = carea,
                        Cvalid = cvalid,
                        Cinvalid = cinvalid,
                        Cstatus = cstatus,
                        Cdate = cdate,
                        Oid = oid
                    };

                    db.contract.Add(newcontract);

                    //修改tenant表的地址为Hid的地址
                    var haddress = db.house.Where(b => b.Hid == hid).Select(b => b.Haddress).FirstOrDefault();
                    var tenant = db.tenant.FirstOrDefault(u => u.Tid == tid);
                    if (tenant != null)
                    {
                        tenant.Haddress = haddress;
                        tenant.Oid = oid;
                    }

                    DateTime time = DateTime.Now;
                    //添加合同时自动添加账单到bill表
                    var newbill = new bill
                    {
                        Tid = tid,
                        Tname = tname,
                        Fname = "房租",
                        Busage = carea,
                        Bprice = cpay,
                        Btime = time,
                        Oid = oid,
                        Bpaid = 0,
                        Bunpaid = cpay
                    };
                    db.bill.Add(newbill);


                    db.SaveChanges();
                    return Content("<script>alert('添加成功！');window.location.href='/Owner/contract_manage';</script>");
                }

               

            }
        }

        //合同信息修改
        [HttpPost]
        public ActionResult contractEdit(FormCollection fc)
        {
            if (fc["tid"] == "" || fc["tname"] == "" || fc["hid"]=="" || fc["cbegintime"] == "" || fc["cendtime"] == "" || fc["cpay"] == "" || fc["caccepted"] == "" || fc["cunaccepted"] == "" || fc["carea"] == "" || fc["cvalid"] == "" || fc["cinvalid"] == "" || fc["cstatus"] == "" || fc["cdate"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {
                int cid = Convert.ToInt32(Session["Cid"]);

                int tid = Convert.ToInt32(fc["tid"]);
                int hid = Convert.ToInt32(fc["hid"]);
                string tname = fc["tname"];
                DateTime cbegintime =Convert.ToDateTime( fc["cbegintime"]);
                DateTime cendtime = Convert.ToDateTime(fc["cendtime"]);
                double cpay = Convert.ToDouble(fc["cpay"]);
                double caccepted = Convert.ToDouble(fc["caccepted"]);
                double cunaccepted = Convert.ToDouble(fc["cunaccepted"]);
                double carea = Convert.ToDouble(fc["carea"]);
                DateTime cvalid = Convert.ToDateTime(fc["cvalid"]);
                DateTime cinvalid = Convert.ToDateTime(fc["cinvalid"]);
                string cstatus = fc["cstatus"];
                DateTime cdate =Convert.ToDateTime( fc["cdate"]);

                string loginname = Session["loginname"].ToString();
                var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

                var contract = db.contract.FirstOrDefault(u => u.Cid == cid);

                if (contract != null)
                {
                    contract.Tid = tid;
                    contract.Tname = tname;
                    contract.Hid = hid;
                    contract.Cbegintime = cbegintime;
                    contract.Cendtime = cendtime;
                    contract.Cpay = cpay;
                    contract.Caccepted = caccepted;
                    contract.Cunaccepted = cunaccepted;
                    contract.Carea = carea;
                    contract.Cvalid = cvalid;
                    contract.Cinvalid = cinvalid;
                    contract.Cstatus = cstatus;
                    contract.Cdate = cdate;

                }

                db.SaveChanges();
                return Content("<script>alert('修改成功！');window.location.href='/Owner/contract_manage';</script>");

            }
        }

        //合同删除
        public ActionResult contractdelete(int? id)
        {
            contract contract = db.contract.Find(id);
            db.contract.Remove(contract);
            db.SaveChanges();
            return Content("<script>alert('删除成功！');window.location.href='/Owner/contract_manage';</script>");
        }
        //合同查询
        [HttpPost]
        public ActionResult SearchByCid(FormCollection fc)
        {
            string keyword = fc["keyword"];
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            if (keyword == "")
            {
                return Content("<script>alert('请输入编码以进行查询');history.go(-1);</script>");
            }
            else
            {
                int no = Convert.ToInt32(keyword);

                var com = db.contract.Where(b => b.Cid == no && b.Oid==oid);

                if (com.Count() > 0)
                {
                    return View("contract_manage", com);
                }
                else
                {
                    return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                }


            }
        }

        //资料修改
        [HttpPost]
        public ActionResult profileEdit(FormCollection fc)
        {
            if (fc["oname"] == "" || fc["sex"] == "0" || fc["ocontact"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {

                string oname = fc["oname"];
                string sex = fc["sex"];
                if (sex == "1")
                {
                    sex = "男";
                }
                else if (sex == "2")
                {
                    sex = "女";
                }
                string ocontact = fc["ocontact"];

                string loginname = Session["loginname"].ToString();
                var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

                var owner = db.owner.FirstOrDefault(u => u.Oid == oid);
                if (owner != null)
                {
                    owner.Oname=oname;
                    owner.Osex=sex;
                    owner.Ocontact=ocontact;
                }

                db.SaveChanges();
                return Content("<script>alert('修改成功！');history.go(-1);</script>");

            }
        }

        //资料完善
        [HttpPost]
        public ActionResult profileComplete(FormCollection fc)
        {
             if (fc["oname"] == "" || fc["sex"] == "0" || fc["ocontact"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {

                string oname = fc["oname"];
                string sex = fc["sex"];
                if (sex == "1")
                {
                    sex = "男";
                }
                else if (sex == "2")
                {
                    sex = "女";
                }
                string ocontact = fc["ocontact"];

               string loginacct= Session["loginacct"].ToString();
               string password = Session["password"].ToString();

                var owner = db.owner.FirstOrDefault(u => u.Oacct==loginacct && u.Opass==password);
                if (owner != null)
                {
                    owner.Oname=oname;
                    owner.Osex=sex;
                    owner.Ocontact=ocontact;
                    owner.Pid = 1;
                }

                db.SaveChanges();
                return Content("<script>alert('资料完善完成，请重新登录');window.location.href='/Home/Index';</script>");

            }
        }


        //重设密码
        [HttpPost]
        public ActionResult passwordreset(FormCollection fc)
        {
            if (fc["oldpass"] == "" || fc["newpass"] == "" || fc["repass"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else if (fc["newpass"] != fc["repass"])
            {
                return Content("<script>alert('密码不一致！');history.go(-1);</script>");
            }
            else
            {

                //添加获取表单信息
                string old = fc["oldpass"];
                string newpass = fc["newpass"];
                string repass = fc["repass"];
                string loginname = Session["loginname"].ToString();
                var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

                var owner = db.owner.FirstOrDefault(u => u.Oid == oid && u.Opass==old);


                if (owner != null)
                {
                    owner.Opass = newpass;
                }
                else
                {
                    return Content("<script>alert('旧密码错误');history.go(-1);</script>");
                }
                db.SaveChanges();
                return Content("<script>alert('密码修改成功！');history.go(-1);</script>");

            }
        }


        //按照用量排序
        public ActionResult billorderbyusage()
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            var com = db.bill.Where(b => b.Oid == oid && b.Fname == "房租").OrderBy(b => b.Busage);

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示所有租户的房租账单";

            return View("bill_search_del_edit", com);
        }
        public ActionResult billorderbyusagedes()
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            var com = db.bill.Where(b => b.Oid == oid && b.Fname == "房租").OrderByDescending(b=>b.Busage);

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示所有租户的房租账单";

            return View("bill_search_del_edit", com);
        }

        //根据价格排序
        public ActionResult billorderbyprice()
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            var com = db.bill.Where(b => b.Oid == oid && b.Fname == "房租").OrderBy(b => b.Bprice);

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示所有租户的房租账单";

            return View("bill_search_del_edit", com);
        }
        public ActionResult billorderbypricedes()
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            var com = db.bill.Where(b => b.Oid == oid && b.Fname == "房租").OrderByDescending(b=>b.Bprice);

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示所有租户的房租账单";

            return View("bill_search_del_edit", com);
        }

        //根据日期排序
        public ActionResult billorderbydate()
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            var com = db.bill.Where(b => b.Oid == oid && b.Fname == "房租").OrderBy(b => b.Btime);

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示所有租户的房租账单";

            return View("bill_search_del_edit", com);
        }
        public ActionResult billorderbydatedes()
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            var com = db.bill.Where(b => b.Oid == oid && b.Fname == "房租").OrderByDescending(b=>b.Btime);

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示所有租户的房租账单";

            return View("bill_search_del_edit", com);
        }

        //侧边栏页面跳转
        public ActionResult serviceapplication()
        {
            return View(db.service);
        }
        public ActionResult profileEdit()
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            owner owner = db.owner.Find(oid);
            return View("profileEdit",owner);
        }
        public ActionResult passwordreset()
        {
            return View();
        }
        public ActionResult suggestions()
        {
            return View();
        }
        public ActionResult applicaion_search_del_edit()
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            var com = db.application.Where(a=>a.Oid==oid);


            return View("applicaion_search_del_edit",com);
        }
        public ActionResult bill_search_del_edit()
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            var com = db.bill.Where(b=>b.Oid==oid && b.Fname=="房租").OrderBy(b=>b.Tid);

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Oid == oid && s.Fname == "房租").Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示所有租户的房租账单";

            return View("bill_search_del_edit",com);
        }
        public ActionResult contract_manage()
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            var com = db.contract.Where(b=>b.Oid==oid).OrderBy(b=>b.Tid);
            return View("contract_manage",com);
        }
        public ActionResult bill_edit(int? id,string name)
        {
            var bill = db.bill.FirstOrDefault(u => u.Tid == id && u.Fname == name);
            Session["Tid"] = id;
            return View("bill_edit",bill);
        }
        public ActionResult billAdd()
        {
            string loginname = Session["loginname"].ToString();
            var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            var com = db.tenant.Where(t=>t.Oid==oid);
         

            List<dynamic> tidList = new List<dynamic>();
            foreach (var item in com.ToList())
            {
                dynamic dyObj = new ExpandoObject();
                dyObj.Tid = item.Tid;
                dyObj.Tname = item.Tname;
                tidList.Add(dyObj);
            }
            ViewBag.tids = tidList;

            return View(db.feeitems);
        }
        public ActionResult contractAdd()
        {
            return View();
        }
        public ActionResult contractEdit(int? id)
        {
            contract contract = db.contract.Find(id);

            Session["Cid"] = id;

            return View("contractEdit", contract);
        }
        public ActionResult profileComplete()
        {
            return View();
        }



        // GET: /Owner/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            owner owner = db.owner.Find(id);
            if (owner == null)
            {
                return HttpNotFound();
            }
            return View(owner);
        }

        // GET: /Owner/Create
        public ActionResult Create()
        {
            ViewBag.Pid = new SelectList(db.propertycompany, "Pid", "Pacct");
            return View();
        }

        // POST: /Owner/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Oid,Oacct,Opass,Oname,Osex,Ocontact,Osuggestion,Pid")] owner owner)
        {
            if (ModelState.IsValid)
            {
                db.owner.Add(owner);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Pid = new SelectList(db.propertycompany, "Pid", "Pacct", owner.Pid);
            return View(owner);
        }

        // GET: /Owner/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            owner owner = db.owner.Find(id);
            if (owner == null)
            {
                return HttpNotFound();
            }
            ViewBag.Pid = new SelectList(db.propertycompany, "Pid", "Pacct", owner.Pid);
            return View(owner);
        }

        // POST: /Owner/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Oid,Oacct,Opass,Oname,Osex,Ocontact,Osuggestion,Pid")] owner owner)
        {
            if (ModelState.IsValid)
            {
                db.Entry(owner).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Pid = new SelectList(db.propertycompany, "Pid", "Pacct", owner.Pid);
            return View(owner);
        }

        // GET: /Owner/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            owner owner = db.owner.Find(id);
            if (owner == null)
            {
                return HttpNotFound();
            }
            return View(owner);
        }

        // POST: /Owner/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            owner owner = db.owner.Find(id);
            db.owner.Remove(owner);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
