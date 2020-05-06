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
using PMS.Models;
using System.Text;
using Aop.Api;
using PMS.Controllers;
using Aop.Api.Domain;
using Aop.Api.Request;
using Aop.Api.Response;
using System.Collections.Specialized;
using Aop.Api.Util;
using PMS.filter;

namespace PMS.Controllers
{
    [MyCheckFilterAttribute(IsCheck = true)]
    public class TenantController : Controller
    {
        private PMS_EF db = new PMS_EF();

        // GET: /Tenant/
        public ActionResult Index()
        {
            Session["loginname"] = Request.Params["displayname"];

            //新增
            var notice = db.propertycompany.Where(u => u.Pnotice != null & u.Prules != null);
            List<dynamic> oneList = new List<dynamic>();
            foreach (var item in notice.ToList())
            {
                dynamic dyObj = new ExpandoObject();
                dyObj.Pnotice = item.Pnotice;
                dyObj.Prules = item.Prules;
                oneList.Add(dyObj);
            }
            ViewBag.data = oneList;


            var tenant = db.tenant.Include(t => t.bill).Include(t => t.owner);
            return View(tenant.ToList());
        }

        public ActionResult logout()
        {
            Session.Abandon();
            return Content("<script>window.location.href='/Home/Index';</script>");
            //return RedirectToAction("Index", "Home");
        }

        //物业费查询_根据日期查询
        [HttpPost]
        public ActionResult SearchByDate(FormCollection fc)
        {
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

                string loginname = Session["loginname"].ToString();
                var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();

                var com = db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid==tid);
                if (com.Count() > 0)
                {

                    //计算日期区间内的缴纳情况
                    double sum = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == tid).Sum(s => s.Bprice));
                    double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == tid).Sum(s => s.Bpaid));
                    double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == tid).Sum(s => s.Bunpaid));
                    ViewBag.sum = sum;
                    ViewBag.accepted = accepted;
                    ViewBag.unaccepted = unaccepted;

                    return View("billmanage", com);
                }
                else
                {
                    return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                }
            }
        }

        //账单导出
        public ActionResult billoutput(int? id)
        {
            var bill = db.bill.Where(u => u.Tid == id);

            Response.AppendHeader("Content-Disposition", "attachment;filename=bill.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "gb2312";
            Response.ContentEncoding = Encoding.GetEncoding("gb2312");
            System.IO.StringWriter writer = new System.IO.StringWriter();

            writer.Write("租户编码");
            writer.Write("\t");
            writer.Write(id);
            writer.Write("\n");
            writer.Write("租户姓名");
            writer.Write("\t");
            writer.Write(bill.Select(b=>b.Tname).FirstOrDefault());
            writer.Write("\n");
            foreach (bill item in bill)
            {
                writer.Write(" ");
                writer.Write("\t");
                writer.Write(" ");
                writer.Write("\t");
                writer.Write(item.Fname);
                writer.Write("\t");
                writer.Write("用量");
                writer.Write("\t");
                writer.Write(item.Busage);
                writer.Write("\t");
                writer.Write("费用");
                writer.Write("\t");
                writer.Write(item.Bprice);
                writer.WriteLine(); 
            }
            writer.Write("总计");
            writer.Write("\t");
            writer.Write(db.bill.Where(b=>b.Tid==id).Sum(b=>b.Bprice));
            writer.Write("\n");
            writer.Write("已缴");
            writer.Write("\t");
            writer.Write(db.bill.Where(b => b.Tid == id).Sum(b => b.Bpaid));
            writer.Write("\n");
            writer.Write("未缴");
            writer.Write("\t");
            writer.Write(db.bill.Where(b => b.Tid == id).Sum(b => b.Bunpaid));
            writer.Write("\n");
            writer.Write("导出时间");
            writer.Write("\t");
            string time = DateTime.Now.Year.ToString()+"-"+DateTime.Now.Month.ToString()+"-"+DateTime.Now.Day.ToString();
            writer.Write(time);
            writer.Write("\n");

            Response.Write(writer.ToString());
            Response.End();
            return Content("<script>alert('打印成功');history.go(-1);</script>");
        }

        //根据车位编码查询记录
        [HttpPost]
        public ActionResult SearchByPSid(FormCollection fc)
        {
             string keyword = fc["keyword"];


            if (keyword == "")
            {
                return Content("<script>alert('请输入编码以进行查询');history.go(-1);</script>");
            }
            else
            {
                int no = Convert.ToInt32(keyword);

                var com = db.parkingspace.Where(b => b.PSid == no);

                if (com.Count() > 0)
                {
                    return View("parkingspacebooking", com);
                }
                else
                {
                    return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                }


            }
        }

        //查询空闲车位
        [HttpPost]
        public ActionResult SearchEmpty()
        {
             var com = db.parkingspace.Where(b => b.PSstatus == "空闲");

                if (com.Count() > 0)
                {
                    return View("parkingspacebooking", com);
                }
                else
                {
                    return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                }
        }

        //车位预订
        public ActionResult psbooking(int? id)
        {
           
                string loginname = Session["loginname"].ToString();
                var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();

                var has = db.tenant.Where(t=>t.Tid==tid && t.PSid!=null);
                if (has.Count()>0)
                {
                    return Content("<script>alert('预订该车位前请先退订原本已预定的车位哦');window.location.href='/Tenant/parkingspacebooking';</script>");
                }
                else
                {
                    var parkingspace = db.parkingspace.FirstOrDefault(u => u.PSid == id && u.PSstatus == "空闲");
                    if (parkingspace != null)
                    {
                        parkingspace.PSstatus = "已售";

                        var tenant = db.tenant.Where(a => a.Tid == tid).FirstOrDefault();
                        if (tenant != null)
                        {
                            tenant.PSid = id;
                        }

                        //租户预定车位后将费用加入账单
                        var tname = db.tenant.Where(b => b.Tid == tid).Select(b => b.Tname).FirstOrDefault();
                        //获取车位面积,价格
                        var psarea = Convert.ToDouble(db.parkingspace.Where(b => b.PSid == id).Select(b => b.PSarea).FirstOrDefault());
                        var psprice = db.parkingspace.Where(b => b.PSid == id).Select(b => b.PSprice).FirstOrDefault();

                        DateTime time = DateTime.Now;
                        var oid = db.tenant.Where(b => b.Tid == tid).Select(b => b.Oid).FirstOrDefault();

                        var newbill = new bill
                        {
                            Tid = tid,
                            Tname = tname,
                            Fname = "车位费",
                            Busage = psarea,
                            Bprice = psprice,
                            Btime = time,
                            Oid = oid,
                            Bpaid = 0,
                            Bunpaid = psprice
                        };

                        db.bill.Add(newbill);



                        db.SaveChanges();
                        return Content("<script>alert('预订成功！');window.location.href='/Tenant/parkingspacebooking';</script>");
                    }
                    else
                    {
                        return Content("<script>alert('该车位已被预订！');window.location.href='/Tenant/parkingspacebooking';</script>");
                    }
                   
                }


               
        }

      

        //车位退订
        public ActionResult psunbooking(int? id)
        {
              string loginname = Session["loginname"].ToString();
                var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();

                var parkingspace = db.parkingspace.FirstOrDefault(u => u.PSid == id && u.PSstatus=="已售");
                if (parkingspace != null)
                {
                    parkingspace.PSstatus = "空闲";

                }
                else
                {
                    return Content("<script>alert('该车位无人预订不必退订！');window.location.href='/Tenant/parkingspacebooking';</script>");
                }
                var tenant = db.tenant.Where(a => a.Tid == tid).FirstOrDefault();
                if (tenant != null)
                {
                    tenant.PSid = null;
                }
               

                db.SaveChanges();
                return Content("<script>alert('退订成功！');window.location.href='/Tenant/parkingspacebooking';</script>");
        }




        //费用缴纳 调用支付宝api

        // 应用ID,您的APPID
        public static string app_id = "2016101600696346";

        // 支付宝网关
        public static string gatewayUrl = "https://openapi.alipaydev.com/gateway.do";

        // 商户私钥，您的原始格式RSA私钥
        public static string private_key = "MIIEowIBAAKCAQEA3CrpDbbuxs/2Qn4liUCOijXYRlQoIK8OTmGW784WLY7eb5EUe923l+PGCyGnYBZi9eDz7tcrG8xC2fbf3tOE+bXZ1HPaLeD4nahbRzofYXsNSLRX8fkI1aB7jW0W7vpWVjbjI4wjHOJ1ETatNnXMbmxxKrn1P5+gz6/Oo1Y4WKda3mO1NfAMOngDBjM4G+LRNI6ldyd8/Ezc4xsolqyuhFQCECwlxAXQ61UiOcauTcS+VVmXcvRY/Xq2lBVQaSJ7D/FDMd7VETvlZm/8UGkS2ZO6+2GOSStnltXBj+HuyNQf2JQim50bZn4zetbh/6EXt2VfcxBGOrdnjUh6FPi/awIDAQABAoIBACvcYJFa4Da7N1QHzXKKadse3vcjzNq2BSOYTXl4lPJ+g9G2FV6XzPt1ZP7StYVu8EgLiI0MUIo5JxhFFlRNzy/wCnVIny6EowLFh2mpKpdA7GSPiPWrpxbn4bVLBkaVmJ2UUavDPzuB4dCME/XFgfR9pg3c/f6uzlqRq5jelFsUnT4RWVGrbnjmbujbzc9kTvdDa588fsBWLiIgYTLItzmNwnz7lK01KG0hKFlgjFSJRuXt4N5+BtE6RXmjvOTC1hnEwmQtctP/3Ya1BXYO3TpKSFFQGLXNxSerg5iAuVOJfD9Zawu+OP3Dn9jWIB+UQHPnayGX/5/xGWhjuAfwsAECgYEA/NLAen4obOrQeqAsc/kPUblG4t+brGKXXuqCQGUzacTHR3roASSHxGO27Rgmw2HAnxRw+D/obcpVpQaEKC2lmILw8igmnsKeqW/5pFNaCTgqj51fbhqPUeAMgSLnFXO9skQWK6urx/Uo6f3vvKrGpv6GCC3CnYSEBgpLfvej8okCgYEA3u8dwE5GXVgHYTJdiZBGWox9CXEcy6SXt9jwr1jPCHfsLdLLU4Jv0iHIc/vNnBO/0cdnO9HQNPCdGBCGDHU391K/w6+u+gscQ10AJPKe+SMlWoMtOSoXKf/e/dZOk+pdpeCdwsH0/3FmRmuH6UxMAiGAnV5XDAHLWrzFISkU9VMCgYBoYs/sA3jHd7A6YTXZcGT91iTJeY+5/j8HhoXe9qniqseo4Ls39ZBE6vLVM9qVYx/3zqXSKfjak+cGqwkX2bj8nlvDcAZ6GFsQFFabnXqYQeN4xn5nZHn2US54hyOoPNB+8RPCVjAn8DZDXoCEgnJg8sf+Sn6HMPad09RWCQNdGQKBgQCi1IwqdgG3FCDvwVXIsHRylsKNLu0VYPbf9bh2mqs9SDpdjeWs7Uy3cq1y6axYH6SvmLGyY6FryYM0nH0MhGGIaAxg5eUsBQlzum3sjrnGxwD1h3J0mmWo65b4WJu0Ni6IhfM02W4VVcKaFNiEcpHhzI6gYtO5lWXutIpXmiYQuQKBgDhR7zawYNeFBaKRgz+Vr60FmhMBOhBNf85GulfzEmQYHbQzeJ12csRmxHAWpdORmb0cIErSsmchV5Y757W7UTFKOPxCOrIei+PopnNOUnxBnePVklz0KngXjXxfJujzTU13HSePfwtVOervcVBeHYeMcVKIkng/IU9uZ+7lk712";
        // 支付宝公钥,查看地址：https://openhome.alipay.com/platform/keyManage.htm 对应APPID下的支付宝公钥。
        public static string alipay_public_key = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAl80VuzXZMgpMGsnmZWy6ttTEgRnj5hK7Z+7bc1h4brY+3ane14qBuHrgmHbeIYkx0qMhAln/34rGsCaXfssBjR3aHLXIR1tIhVhLN3pz4oTWZfPFbvOV83xwJ/KKGAi8SxXA+gtyDr9mw8OMJ0IihnKA+511YS3gHgjdCAlU5+BJ6VauxgvkevGBlUtCUusFkdcxo5fuQz3Pb5NmSyFSc5yGGh1f7AYdme/hRVjp9jYt3WldqLhv985itaqd+lo4fiTc52OFZzxBBxUnagTojSQGsfFVljXO72VIPle7pvni13zHE1vt/qJHEMZTFEPVcDuBSWsCFAFwqaM2e5OFXQIDAQAB";
        // 签名方式
        public static string sign_type = "RSA2";

        // 编码格式
        public static string charset = "UTF-8";

        //回调地址
        //returnur需要修改为地址+端口号才能跳转，否则只是本地的
        /*
         本地ReturnUrl  http://localhost:3979/Tenant/ReturnUrl
         * 本地NotifyUrl  http://localhost:3979/Tenant/PayNotify
         
         * ReturnUrl:http://122.51.44.245:2333/Tenant/ReturnUrl
         * NotifyUrl:http://122.51.44.245:2333/Tenant/PayNotify
         * 
         */
        public static string AliPayReturn_url = "http://139.196.200.159:9512/Tenant/ReturnUrl";
        public static string AliPayNotify_url = "http://139.196.200.159:9512/Tenant/PayNotify";

        //那些变量：
        public static string Tname = "";
        public static string Fname = "";
        public static float Bprice = 0;
        public static int Tid = 0;

        //页面跳转
        public ActionResult billpay(int? id, string name, float price,string tname)
        {
            var unpaid = db.bill.Where(b => b.Tid == id && b.Fname == name).Select(b => b.Bunpaid).FirstOrDefault();
            if (unpaid == 0)
            {
                return Content("<script>alert('该项已缴纳，不必重复缴纳哦');history.go(-1);</script>");
            }

            else
            {
                //将信息查询并放入session域中
                Fname = name;
                Tname = tname;
                Tid = (int)id;

                Bprice = price;
                Session["bprice"] = Bprice;

                ViewBag.subject = Tname + "[" + Fname + "]的缴纳订单";
                ViewBag.detail = Fname;
                return View();
            }

        }
        //支付界面完成支付
        [HttpPost]
        public ActionResult billpay()
        {
           
            
                DefaultAopClient client = new DefaultAopClient(gatewayUrl, app_id, private_key, "json", "1.0", "RSA2", alipay_public_key, "utf-8", false);

                // 外部订单号，商户网站订单系统中唯一的订单号
                string out_trade_no = DateTime.Now.ToString("yyyyMMddHHmmss");

                // 订单名称
                string subject = Tname + "[" + Fname + "]的缴纳订单";

                // 付款金额
                string total_amout = Convert.ToString(Bprice);

                // 商品描述
                string body = "该缴纳项目为" + Fname;

                // 组装业务参数model
                AlipayTradePagePayModel model = new AlipayTradePagePayModel();
                model.Body = body;
                model.Subject = subject;
                model.TotalAmount = total_amout;
                model.OutTradeNo = out_trade_no;
                model.ProductCode = "FAST_INSTANT_TRADE_PAY";

                AlipayTradePagePayRequest request = new AlipayTradePagePayRequest();
                // 设置同步回调地址
                request.SetReturnUrl(AliPayReturn_url);
                // 设置异步通知接收地址
                request.SetNotifyUrl(AliPayNotify_url);
                // 将业务model载入到request
                request.SetBizModel(model);

                AlipayTradePagePayResponse response = null;
                try
                {
                    response = client.pageExecute(request, null, "post");
                    Response.Write(response.Body);
                }
                catch (Exception exp)
                {
                    throw exp;
                }


                return View();
            
        }


        public Dictionary<string, string> GetRequestGet()
        {
            int i = 0;
            Dictionary<string, string> sArray = new Dictionary<string, string>();
            NameValueCollection coll;
            //coll = Request.Form;
            coll = Request.QueryString;
            String[] requestItem = coll.AllKeys;
            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.QueryString[requestItem[i]]);
            }
            return sArray;

        }

        //回调页面
        public ActionResult ReturnUrl()
        {
            Dictionary<string, string> sArray = GetRequestGet();
            int Result = 0;
            if (sArray.Count > 0)//判断是否有带返回参数
            {
                bool flag = AlipaySignature.RSACheckV1(sArray, alipay_public_key, "utf-8", "RSA2", false);

                if (flag)//验证成功
                {
                    var bill = db.bill.FirstOrDefault(u => u.Tid == Tid && u.Fname==Fname);
                    if (bill != null)
                    {
                        bill.Bpaid = Bprice;
                        bill.Bunpaid = 0;
                        ViewBag.pagestatus = "当前显示全部账单";


                        db.SaveChanges();

                        return Content("<script>alert('支付完成');window.location.href='/Tenant/billmanage'</script>");
                    }

                   
                }
                else//验证失败
                {
                    Result = 0;
                    // Cmn.Log.Write("异步支付验证失败！" + Request.Form);
                }
            }
            else
            {
                ViewBag.pagestatus = "当前显示全部账单";
                return Content("<script>window.location.href='/Tenant/billmanage'</script>");
            }
            return View();
        }

        //以下是服务器需求，无法在电脑本机完成测试
        // 获取支付宝POST过来通知消息，并以“参数名=参数值”的形式组成数组
        public IDictionary<string, string> GetRequestPost()
        {
            int i = 0;
            IDictionary<string, string> sArray = new Dictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.Form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.Form[requestItem[i]]);
            }
            return sArray;
        }

        public ActionResult PayNotify()
        {
            // 获取支付宝Post过来反馈信息
            IDictionary<string, string> map = GetRequestPost();
            if (map.Count > 0) //判断是否有带返回参数
            {
                try
                {
                    //支付宝的公钥
                    string alipayPublicKey = alipay_public_key;
                    string signType = sign_type;
                    string charset = "UTF-8";
                    bool keyFromFile = false;

                    bool verify_result = AlipaySignature.RSACheckV1(map, alipayPublicKey, charset, signType, keyFromFile);
                    // 验签成功后，按照支付结果异步通知中的描述，对支付结果中的业务内容进行二次校验，校验成功后在response中返回success并继续商户自身业务处理，校验失败返回failure
                    if (verify_result)
                    {
                        //商户订单号
                        string out_trade_no = map["out_trade_no"];
                        //支付宝交易号
                        string trade_no = map["trade_no"];
                        //交易创建时间
                        string gmt_create = map["gmt_create"];
                        //交易付款时间
                        string gmt_payment = map["gmt_payment"];
                        //通知时间
                        string notify_time = map["notify_time"];
                        //通知类型  trade_status_sync
                        string notify_type = map["notify_type"];
                        //通知校验ID
                        string notify_id = map["notify_id"];
                        //开发者的app_id
                        string app_id = map["app_id"];
                        //卖家支付宝用户号
                        string seller_id = map["seller_id"];
                        //买家支付宝用户号
                        string buyer_id = map["buyer_id"];
                        //实收金额
                        string receipt_amount = map["receipt_amount"];
                        //交易状态
                        //交易状态TRADE_FINISHED的通知触发条件是商户签约的产品不支持退款功能的前提下，买家付款成功；
                        //或者，商户签约的产品支持退款功能的前提下，交易已经成功并且已经超过可退款期限
                        //状态TRADE_SUCCESS的通知触发条件是商户签约的产品支持退款功能的前提下，买家付款成功
                        if (map["trade_status"] == "TRADE_FINISHED" || map["trade_status"] == "TRADE_SUCCESS")
                        {

                            var bill = db.bill.FirstOrDefault(u => u.Tid == Tid && u.Fname == Fname);
                            if (bill != null)
                            {
                                bill.Bpaid = Bprice;
                                bill.Bunpaid = 0;
                                ViewBag.pagestatus = "当前显示全部账单";
                                db.SaveChanges();
                                Response.Write("success");  //请不要修改或删除
                                return Content("<script>alert('支付完成');window.location.href='/Tenant/billmanage'</script>");
                            }

                            /*
                               //判断该笔订单是否在商户网站中已经做过处理
                            DataTable dd = collegeService.OrderPayNot(out_trade_no).Tables[0];
                            if (Convert.ToInt32(dd.Rows[0]["Status"]) == 0)
                            {
                                //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                                #region 将数据提添加到集合中
                                Dictionary<string, string> myDic = new Dictionary<string, string>();
                                myDic.Add("PayTradeNo", trade_no);
                                myDic.Add("Status", "1");
                                myDic.Add("Type", "0");
                                myDic.Add("PayTime", gmt_payment);
                                myDic.Add("BuyerId", buyer_id);
                                myDic.Add("OrderNo", out_trade_no);
                                #endregion

                                #region 添加数据到数据库
                                bool res = collegeService.AddPayInfo(myDic);
                                if (res == false)
                                {
                                    Response.Write("添加支付信息失败!");
                                }
                                #endregion

                                Response.Write("success");  //请不要修改或删除
                            }
                             */
                            else
                            {
                                ViewBag.pagestatus = "当前显示全部账单";
                                return Content("<script>alert('发生未知错误>_<');window.location.href='/Tenant/billmanage'</script>");
                            }

                        }
                    }
                    // 验签失败则记录异常日志，并在response中返回failure.
                    else
                    {
                        Response.Write("验证失败");
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            else
            {
                Response.Write("无返回参数");
            }
            return View("billmanage");
        }


        public ActionResult pagepay()
        {
            return View();
        }

        //修改个人资料
        [HttpPost]
        public ActionResult profileEdit(FormCollection fc)
        {
            if (fc["tname"] == "" || fc["idtype"] == "0" ||fc["tcontact"]==""||fc["tidno"]==""||fc["sex"]=="0")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {

                string tname = fc["tname"];
                string sex = fc["sex"];
                if (sex == "1")
                {
                    sex = "男";
                }
                else if (sex == "2")
                {
                    sex = "女";
                }
                string tcontact = fc["tcontact"];
                string tidno = fc["tidno"];
                string idtype = fc["idtype"];
                if (idtype == "1")
                {
                    idtype = "大陆身份证";
                }
                else if (idtype == "2")
                {
                    idtype = "护照";
                }
                else
                {
                    idtype = "港澳台身份证";
                }

                string loginname = Session["loginname"].ToString();
                var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();

                var tenant = db.tenant.FirstOrDefault(b=>b.Tid==tid);
                if (tenant != null)
                {
                    tenant.Tname = tname;
                    tenant.Tsex = sex;
                    tenant.Tcontact = tcontact;
                    tenant.Tidno = tidno;
                    tenant.Tidtype = idtype;
                }

                db.SaveChanges();
                return Content("<script>alert('修改成功！');history.go(-1);</script>");

            }
        }

        //资料完善
        [HttpPost]
        public ActionResult profileComplete(FormCollection fc)
        {
             if (fc["tname"] == "" || fc["idtype"] == "0" ||fc["tcontact"]==""||fc["tidno"]==""||fc["sex"]=="0")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {

                string tname = fc["tname"];
                string sex = fc["sex"];
                if (sex == "1")
                {
                    sex = "男";
                }
                else if (sex == "2")
                {
                    sex = "女";
                }
                string tcontact = fc["tcontact"];
                string tidno = fc["tidno"];
                string idtype = fc["idtype"];
                if (idtype == "1")
                {
                    idtype = "大陆身份证";
                }
                else if (idtype == "2")
                {
                    idtype = "护照";
                }
                else
                {
                    idtype = "港澳台身份证";
                }

               string loginacct= Session["loginacct"].ToString();
               string password = Session["password"].ToString();

                var tenant = db.tenant.FirstOrDefault(b=>b.Tacct==loginacct && b.Tpass==password);
                if (tenant != null)
                {
                    tenant.Tname = tname;
                    tenant.Tsex = sex;
                    tenant.Tcontact = tcontact;
                    tenant.Tidno = tidno;
                    tenant.Tidtype = idtype;
                }

                db.SaveChanges();
                return Content("<script>alert('资料已完善，请重新登录');window.location.href='/Home/Index';</script>");

            }
        }



        //修改密码
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
                var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();

                 var tenant = db.tenant.FirstOrDefault(b=>b.Tid==tid && b.Tpass==old);

                if (tenant != null)
                {
                    tenant.Tpass = newpass;
                }
                else
                {
                    return Content("<script>alert('旧密码错误');history.go(-1);</script>");
                }
                db.SaveChanges();
                return Content("<script>alert('密码修改成功！');history.go(-1);</script>");

            }
        }
        //组合查询
        [HttpPost]
        public ActionResult CombineSearch(FormCollection fc)
        {
            string loginname = Session["loginname"].ToString();
            var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();

            if (fc["pay"] == "" & fc["date1"] == "" & fc["date2"] == "")
            {
                return Content("<script>alert('请至少选择一项进行查询');history.go(-1);</script>");
            }
            else
            {
                if (fc["pay"] != "" & fc["date1"] == "" & fc["date2"] == "")//已缴/未缴
                {
                    if (fc["pay"] == "未缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid != 0 && b.Tid==tid);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b =>  b.Tid==tid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Tid == tid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Tid == tid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示未缴账单";

                        return View("billmanage", com);
                    }
                    else if (fc["pay"] == "已缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid == 0 && b.Tid == tid);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Tid == tid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Tid == tid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Tid == tid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示已缴账单";


                        return View("billmanage", com);
                    }
                    else
                    {
                        return Content("<script>alert('未知错误>_<');history.go(-1);</script>");
                    }
                }
                else if (fc["pay"] != "" & fc["date1"] != "" & fc["date2"] != "")//已缴未缴+日期
                {
                    string d1 = fc["date1"];
                    string d2 = fc["date2"];

                    DateTime dt1 = Convert.ToDateTime(d1);
                    DateTime dt2 = Convert.ToDateTime(d2);

                    if (fc["pay"] == "未缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid != 0 && b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == tid);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == tid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == tid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == tid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示在" + d1 + "-" + d2 + "之间未缴的账单";

                        return View("billmanage", com);
                    }
                    else if (fc["pay"] == "已缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid == 0 && b.Btime > dt1 && b.Btime < dt2 && b.Tid == tid);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Btime > dt1 && b.Btime < dt2 && b.Tid == tid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime > dt1 && b.Btime < dt2 && b.Tid == tid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime > dt1 && b.Btime < dt2 && b.Tid == tid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示在" + d1 + "-" + d2 + "之间已缴的账单";

                        return View("billmanage", com);
                    }
                    else
                    {
                        return Content("<script>alert('未知错误>_<');history.go(-1);</script>");
                    }

                }
                
                else if (fc["pay"] == "" & fc["date1"] != "" & fc["date2"] != "" )//日期
                {
                    string d1 = fc["date1"];
                    string d2 = fc["date2"];

                    DateTime dt1 = Convert.ToDateTime(d1);
                    DateTime dt2 = Convert.ToDateTime(d2);

                    var com = db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == tid);
                    if (com.Count() > 0)
                    {
                        //计算该日期区间的缴纳情况
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == tid).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == tid).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == tid).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示在" + d1 + "-" + d2 + "之间的账单";

                        return View("billmanage", com);
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





        //未缴已缴查看
        public ActionResult unpaidpage()
        {
             string loginname = Session["loginname"].ToString();
            var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();

            var com = db.bill.Where(b => b.Tid == tid && b.Bunpaid != 0);

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            return View("billmanage", com);
        }
        public ActionResult paidpage()
        {
            string loginname = Session["loginname"].ToString();
            var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();

            var com = db.bill.Where(b => b.Tid == tid && b.Bunpaid == 0);

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;


            return View("billmanage", com);
        }

        //按照用量排序
        public ActionResult billorderbyusage()
        {
            string loginname = Session["loginname"].ToString();
            var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();


            var bill = db.bill.Where(u => u.Tid == tid).OrderBy(t=>t.Busage);
           

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示全部账单";

            Session["Tid"] = tid;
            return View("billmanage", bill);
        }
        public ActionResult billorderbyusagedes()
        {
            string loginname = Session["loginname"].ToString();
            var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();


            var bill = db.bill.Where(u => u.Tid == tid).OrderByDescending(t=>t.Busage);


            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示全部账单";

            Session["Tid"] = tid;
            return View("billmanage", bill);
        }

        //根据价格排序
        public ActionResult billorderbyprice()
        {
            string loginname = Session["loginname"].ToString();
            var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();


            var bill = db.bill.Where(u => u.Tid == tid).OrderBy(t => t.Bprice);


            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示全部账单";

            Session["Tid"] = tid;
            return View("billmanage", bill);
        }
        public ActionResult billorderbypricedes()
        {
            string loginname = Session["loginname"].ToString();
            var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();


            var bill = db.bill.Where(u => u.Tid == tid).OrderByDescending(t=>t.Bprice);


            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示全部账单";

            Session["Tid"] = tid;
            return View("billmanage", bill);
        }

        //根据日期排序
        public ActionResult billorderbydate()
        {
            string loginname = Session["loginname"].ToString();
            var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();


            var bill = db.bill.Where(u => u.Tid == tid).OrderBy(t => t.Btime);


            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示全部账单";

            Session["Tid"] = tid;
            return View("billmanage", bill);
        }
        public ActionResult billorderbydatedes()
        {
            string loginname = Session["loginname"].ToString();
            var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();


            var bill = db.bill.Where(u => u.Tid == tid).OrderByDescending(t=>t.Btime);


            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示全部账单";

            Session["Tid"] = tid;
            return View("billmanage", bill);
        }

        //按照车位价格排序
        public ActionResult orderbyprice()
        {
            string loginname = Session["loginname"].ToString();
            var psid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.PSid).FirstOrDefault();
            if (psid == null)
            {
                ViewBag.data = "您当前未预订任何车位";
            }
            else
            {
                var pslocation = db.parkingspace.Where(ps => ps.PSid == psid).Select(ps => ps.PSlocation).FirstOrDefault();
                ViewBag.data = "您当前预订的车位为:" + Convert.ToString(pslocation);
            }


            return View("parkingspacebooking", db.parkingspace.OrderBy(p => p.PSprice));
        }
        public ActionResult orderbypricedes()
        {
            string loginname = Session["loginname"].ToString();
            var psid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.PSid).FirstOrDefault();
            if (psid == null)
            {
                ViewBag.data = "您当前未预订任何车位";
            }
            else
            {
                var pslocation = db.parkingspace.Where(ps => ps.PSid == psid).Select(ps => ps.PSlocation).FirstOrDefault();
                ViewBag.data = "您当前预订的车位为:" + Convert.ToString(pslocation);
            }


            return View("parkingspacebooking", db.parkingspace.OrderByDescending(p => p.PSprice));
        }

        public ActionResult orderbyarea()
        {
            string loginname = Session["loginname"].ToString();
            var psid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.PSid).FirstOrDefault();
            if (psid == null)
            {
                ViewBag.data = "您当前未预订任何车位";
            }
            else
            {
                var pslocation = db.parkingspace.Where(ps => ps.PSid == psid).Select(ps => ps.PSlocation).FirstOrDefault();
                ViewBag.data = "您当前预订的车位为:" + Convert.ToString(pslocation);
            }


            return View("parkingspacebooking", db.parkingspace.OrderBy(p => p.PSarea));
        }
        public ActionResult orderbyareades()
        {
            string loginname = Session["loginname"].ToString();
            var psid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.PSid).FirstOrDefault();
            if (psid == null)
            {
                ViewBag.data = "您当前未预订任何车位";
            }
            else
            {
                var pslocation = db.parkingspace.Where(ps => ps.PSid == psid).Select(ps => ps.PSlocation).FirstOrDefault();
                ViewBag.data = "您当前预订的车位为:" + Convert.ToString(pslocation);
            }


            return View("parkingspacebooking", db.parkingspace.OrderByDescending(p => p.PSarea));
        }




        //侧栏页面跳转
        public ActionResult billmanage()
        {
            string loginname = Session["loginname"].ToString();
            var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();

            //获取所属业主的付款码
            var oid = db.tenant.Where(b => b.Tid == tid).Select(b => b.Oid).FirstOrDefault();
            var url = db.owner.Where(b => b.Oid == oid).Select(b => b.Oqrcode).FirstOrDefault();
            ViewBag.qrcode = url;

            var bill = db.bill.Where(u => u.Tid == tid );
           // decimal sum = db.bill.Where(u => u.Tid == tid).ToList().Sum(u => u.Bprice);

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Where(s => s.Tid == tid).Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示全部账单";

            Session["Tid"] = tid;
            return View("billmanage", bill);
            
        }
        public ActionResult profileEdit()
        {
            string loginname = Session["loginname"].ToString();
            var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();

            tenant tenant = db.tenant.Find(tid);
            return View("profileEdit", tenant);
        }
        public ActionResult passwordreset()
        {
            return View();
        }
        public ActionResult parkingspacebooking()
        {
            string loginname = Session["loginname"].ToString();
            var psid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.PSid).FirstOrDefault();
            if (psid == null)
            {
                ViewBag.data = "您当前未预订任何车位";
            }
            else
            {
                var pslocation = db.parkingspace.Where(ps => ps.PSid == psid).Select(ps => ps.PSlocation).FirstOrDefault();
                ViewBag.data = "您当前预订的车位为:" + Convert.ToString(pslocation);
            }
            

            return View(db.parkingspace);
        }
        public ActionResult contract_manage()
        {
            string loginname = Session["loginname"].ToString();
            var tid = db.tenant.Where(b => b.Tname == loginname).Select(b => b.Tid).FirstOrDefault();

            var contract = db.contract.Where(b=>b.Tid==tid);
            Session["Tid"] = tid;
            return View("contract_manage", contract);


        }
        public ActionResult profileComplete()
        {
            return View();
        }




        // GET: /Tenant/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tenant tenant = db.tenant.Find(id);
            if (tenant == null)
            {
                return HttpNotFound();
            }
            return View(tenant);
        }

        // GET: /Tenant/Create
        public ActionResult Create()
        {
            ViewBag.Tid = new SelectList(db.bill, "Tid", "Fname");
            ViewBag.Oid = new SelectList(db.owner, "Oid", "Oacct");
            return View();
        }

        // POST: /Tenant/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Tid,Tacct,Tpass,Tname,Tsex,Tidno,Tidtype,Tcontact,Haddress,PSid,Oid")] tenant tenant)
        {
            if (ModelState.IsValid)
            {
                db.tenant.Add(tenant);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Tid = new SelectList(db.bill, "Tid", "Fname", tenant.Tid);
            ViewBag.Oid = new SelectList(db.owner, "Oid", "Oacct", tenant.Oid);
            return View(tenant);
        }

        // GET: /Tenant/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tenant tenant = db.tenant.Find(id);
            if (tenant == null)
            {
                return HttpNotFound();
            }
            ViewBag.Tid = new SelectList(db.bill, "Tid", "Fname", tenant.Tid);
            ViewBag.Oid = new SelectList(db.owner, "Oid", "Oacct", tenant.Oid);
            return View(tenant);
        }

        // POST: /Tenant/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Tid,Tacct,Tpass,Tname,Tsex,Tidno,Tidtype,Tcontact,Haddress,PSid,Oid")] tenant tenant)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tenant).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Tid = new SelectList(db.bill, "Tid", "Fname", tenant.Tid);
            ViewBag.Oid = new SelectList(db.owner, "Oid", "Oacct", tenant.Oid);
            return View(tenant);
        }

        // GET: /Tenant/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tenant tenant = db.tenant.Find(id);
            if (tenant == null)
            {
                return HttpNotFound();
            }
            return View(tenant);
        }

        // POST: /Tenant/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tenant tenant = db.tenant.Find(id);
            db.tenant.Remove(tenant);
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
