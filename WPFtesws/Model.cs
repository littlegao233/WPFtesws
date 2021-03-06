﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PFWebsocketAPI
{

    //internal static class PackOperation
    //{
    //    /// <summary>
    //    /// 读取元数据包
    //    /// </summary>
    //    /// <param name="message"></param>
    //    internal static void ReadPackInitial(string message, object con)
    //    {
    //        try
    //        {
    //            ReadPack(message, true, con);
    //        }
    //        catch (JsonReaderException ex)
    //        {
    //            WSACT.SendToCon(con, new CauseDecodeFailed("格式错误:" + ex.Message).ToString());
    //        }
    //        catch (Exception ex)
    //        {
    //            WSACT.SendToCon(con, new CauseDecodeFailed("解析错误:" + ex.Message).ToString());
    //            PFWebsocketAPI.Program.WriteLineERR("解析错误", ex.ToString());
    //        }
    //    }

    //    internal static void ReadPackNext(string message, object con)
    //    {
    //        ReadPack(message, false, con);
    //    }

    //    internal static void ReadPack(string message, bool IsFirstLayer, object con)
    //    {
    //        var jobj = JObject.Parse(message);
    //        switch (Enum.Parse(typeof(PackType), jobj.Value<string>("type"), true))
    //        {
    //            case PackType.encrypted: // 作为加密包进行解密
    //                {
    //                    ReadEncryptedPack(jobj, IsFirstLayer, con);
    //                    break;
    //                }

    //            case PackType.pack: // 作为普通包解析
    //                {
    //                    ReadOriginalPack(jobj, IsFirstLayer, con);
    //                    break;
    //                }
    //        }
    //    }

    //    internal static void ReadEncryptedPack(JObject jobj, bool IsFirstLayer, object con)
    //    {
    //        {
    //            var withBlock = new EncryptedPack(jobj);
    //            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
    //            string decoded;
    //            try
    //            {
    //                decoded = withBlock.Decode(WSBASE.Config.Password);
    //            }
    //            catch (Exception ex)
    //            {
    //                var fb = new CauseDecodeFailed("密匙验证失败！");
    //                PFWebsocketAPI.Program.WriteLine("密文解密失败，请检查密匙是否正确!");
    //                WSACT.SendToCon(con, fb.ToString()); // 直接返回
    //                return;
    //            }

    //            ReadPackNext(decoded, con); // 嵌套方法
    //        }
    //    }

    //    internal static void ReadOriginalPack(JObject jobj, bool IsFirstLayer, object con)
    //    {
    //        /* TODO ERROR: Skipped IfDirectiveTrivia */
    //        if (IsFirstLayer) // 判断初始包，如果是未加密的初始包则不允许执行
    //        {
    //            var fb = new CauseInvalidRequest("未加密的初始包不予执行！");
    //            PFWebsocketAPI.Program.WriteLine("未加密的初始包不予执行!");
    //            WSACT.SendToCon(con, fb.ToString()); // 直接返回
    //            return;
    //        }
    //        /* TODO ERROR: Skipped EndIfDirectiveTrivia */
    //        switch (Enum.Parse(typeof(ClientActionType), jobj.Value<string>("action"), true))
    //        {
    //            case ClientActionType.runcmdrequest:
    //                {
    //                    {
    //                        var withBlock = new ActionRunCmd(jobj, con);
    //                        var fb = withBlock.GetFeedback();
    //                        if (withBlock.@params.cmd.StartsWith("op ") || withBlock.@params.cmd.StartsWith("execute") && withBlock.@params.cmd.IndexOf("op ") != -1)
    //                        {
    //                            fb.@params.result = "出于安全考虑，禁止远程执行op命令";
    //                            PFWebsocketAPI.Program.WriteLine("出于安全考虑，禁止远程执行op命令");
    //                            WSACT.SendToCon(fb.ToString(), Conversions.ToString(con)); // 直接返回
    //                        }

    //                        PFWebsocketAPI.Program.cmdQueue.Enqueue(fb);
    //                        PFWebsocketAPI.Program.CmdTimer_Elapsed(PFWebsocketAPI.Program.cmdTimer, null);
    //                        if (!PFWebsocketAPI.Program.cmdTimer.Enabled)
    //                            PFWebsocketAPI.Program.cmdTimer.Start();
    //                    }

    //                    break;
    //                }

    //            case ClientActionType.broadcast:
    //                {
    //                    break;
    //                }

    //            case ClientActionType.tellraw:
    //                {
    //                    break;
    //                }
    //        }
    //    }
    //}
}

namespace PFWebsocketAPI.PFWebsocketAPI.Model
{
    public static class StringTools
    {
        public static string GetMD5(string sDataIn)
        {
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bytValue, bytHash;
            bytValue = System.Text.Encoding.UTF8.GetBytes(sDataIn);
            bytHash = md5.ComputeHash(bytValue);
            md5.Clear();
            string sTemp = "";
            for (int i = 0, loopTo = bytHash.Length - 1; i <= loopTo; i++)
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            return sTemp.ToUpper();
        }

        public static string AESEncrypt(string content, string password)
        {
            string md5 = GetMD5(password);
            string iv = md5.Substring(16);
            string key = md5.Remove(16);
            return EasyEncryption.AES.Encrypt(content, key, iv);
        }

        public static string AESDecrypt(string content, string password)
        {
            string md5 = GetMD5(password);
            string iv = md5.Substring(16);
            string key = md5.Remove(16);
            return EasyEncryption.AES.Decrypt(content, key, iv);
        }
    }

    public class Vec3 { public float x, y, z; }
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))] // 基本包类型
    public enum PackType
    {
        pack,
        encrypted
    }

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))] // 加密模式
    public enum EncryptionMode
    {
        aes256,
        aes_cbc_pck7padding
    }

    internal abstract class PackBase // 基础类
    {
        [JsonProperty(Order = -3)]
        public abstract PackType type { get; } // 包类型

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this); // 基类的转化为String重写方法
        }

        public T GetParams<T>(JObject json)
        {
            return json["params"].ToObject<T>(); // 基类的获取参数表方法
        }
    }

    internal class EncryptedPack : PackBase    // 加密包
    {

        public override PackType type { get; } = PackType.encrypted;

        public ParamMap @params;

        internal EncryptedPack(JObject json) // 通过已有json初始化对象（通常用作传入解析）
        {
            @params = GetParams<ParamMap>(json); // 通过基类该方法获取参数表
        }

        internal EncryptedPack(EncryptionMode mode, string from, string password) // 通过参数初始化包（通常用作发送前）
        {
            string encrypted = "";
            switch (mode)// 不同加密模式不同操作
            {
                case EncryptionMode.aes256:
                    {
                        encrypted = SimpleAES.AES256.Encrypt(from, password);
                        break;
                    }
                case EncryptionMode.aes_cbc_pck7padding:
                    {
                        encrypted = (StringTools.AESEncrypt(from, password));
                        break;
                    }
            }

            @params = new ParamMap() { mode = mode, raw = encrypted };
        }

        public string Decode(string password) // 解密params.raw中的内容并返回
        {
            string decrypted = "";
            switch (@params.mode)// 不同加密模式不同操作
            {
                case EncryptionMode.aes256:
                    {
                        decrypted = SimpleAES.AES256.Decrypt(@params.raw, password);
                        break;
                    }

                case EncryptionMode.aes_cbc_pck7padding:
                    {
                        decrypted = (StringTools.AESDecrypt(@params.raw, password));
                        break;
                    }
            }

            if (string.IsNullOrEmpty(decrypted))
                throw new Exception("AES Decode failed!");
            return decrypted;
        }

        internal class ParamMap // 对象参数表
        {
            public EncryptionMode mode;
            public string raw;
        }
    }

    internal class OriginalPack : PackBase   // 普通包/解密后的包
    {
        public override PackType type { get; } = PackType.pack;
    }

    /* TODO ERROR: Skipped RegionDirectiveTrivia */
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum ServerCauseType
    {
        chat,
        join,
        left,
        cmd,
        mobdie,
        runcmdfeedback,
        decodefailed,
        invalidrequest
    }

    internal abstract class ServerPackBase : OriginalPack
    {
        // <JsonProperty("cause")>
        [JsonProperty(Order = -2)]
        public abstract ServerCauseType cause { get; }
    }

    internal class CauseJoin : ServerPackBase
    {
        internal CauseJoin(JObject json)
        {
            @params = GetParams<ParamMap>(json);
        }

        internal CauseJoin(string sender, string xuid, string uuid, string ip)
        {
            @params = new ParamMap() { sender = sender, xuid = xuid, uuid = uuid, ip = ip };
        }

        public override ServerCauseType cause { get; } = ServerCauseType.join;

        public ParamMap @params;

        internal class ParamMap
        {
            public string sender, xuid, uuid, ip;
        }
    }

    internal class CauseLeft : ServerPackBase
    {
        internal CauseLeft(JObject json)
        {
            @params = GetParams<ParamMap>(json);
        }

        internal CauseLeft(string sender, string xuid, string uuid, string ip)
        {
            @params = new ParamMap() { sender = sender, xuid = xuid, uuid = uuid, ip = ip };
        }

        public override ServerCauseType cause { get; } = ServerCauseType.left;

        public ParamMap @params;

        internal class ParamMap
        {
            public string sender, xuid, uuid, ip;
        }
    }

    internal class CauseChat : ServerPackBase
    {
        internal CauseChat(JObject json)
        {
            @params = GetParams<ParamMap>(json);
        }

        internal CauseChat(string sender, string text)
        {
            @params = new ParamMap() { sender = sender, text = text };
        }

        public override ServerCauseType cause { get; } = ServerCauseType.chat;

        public ParamMap @params;

        internal class ParamMap
        {
            public string sender, text;
        }
    }
    internal class CauseCmd : ServerPackBase
    {
        internal CauseCmd(JObject json)
        {
            @params = GetParams<ParamMap>(json);
        }
        internal CauseCmd(string sender, string text)
        {
            @params = new ParamMap() { sender = sender, text = text };
        }
        public override ServerCauseType cause { get; } = ServerCauseType.cmd;
        public ParamMap @params;
        internal class ParamMap
        {
            public string sender, text;
        }
    }
    internal class CauseMobDie : ServerPackBase
    {
        internal CauseMobDie(JObject json)
        {
            @params = GetParams<ParamMap>(json);
        }

        internal CauseMobDie(string mobtype, string mobname, int dmcase, string srctype, string srcname, Vec3 pos)
        {
            @params = new ParamMap() { mobname=mobname,mobtype=mobtype,dmcase=dmcase,srctype=srctype,srcname=srcname,pos=pos};
        }

        public override ServerCauseType cause { get; } = ServerCauseType.mobdie;

        public ParamMap @params;

        internal class ParamMap
        {
            public int dmcase;
            public string mobtype, mobname, srctype, srcname;
            public Vec3 pos;
        }
    }
    // 命令返回
    internal class CauseRuncmdFeedback : ServerPackBase
    {
        internal CauseRuncmdFeedback(JObject json)
        {
            @params = GetParams<ParamMap>(json);
        }

        internal CauseRuncmdFeedback(string id, string cmd, string result, object con)
        {
            @params = new ParamMap() { id = id, cmd = cmd, result = result, con = con };
        }

        public override ServerCauseType cause { get; } = ServerCauseType.runcmdfeedback;

        public ParamMap @params;

        internal class ParamMap
        {
            public string id;
            public string result;
            [JsonIgnore]
            internal string cmd;
            [JsonIgnore]
            internal int waiting = 0;
            [JsonIgnore]
            internal object con;
        }
    }

    internal class CauseDecodeFailed : ServerPackBase
    {
        internal CauseDecodeFailed(JObject json)
        {
            @params = GetParams<ParamMap>(json);
        }

        internal CauseDecodeFailed(string msg)
        {
            @params = new ParamMap() { msg = msg };
        }

        public override ServerCauseType cause { get; } = ServerCauseType.decodefailed;

        public ParamMap @params;

        internal class ParamMap
        {
            public string msg;
        }
    }

    internal class CauseInvalidRequest : ServerPackBase
    {
        internal CauseInvalidRequest(JObject json)
        {
            @params = GetParams<ParamMap>(json);
        }

        internal CauseInvalidRequest(string msg)
        {
            @params = new ParamMap() { msg = msg };
        }

        public override ServerCauseType cause { get; } = ServerCauseType.invalidrequest;

        public ParamMap @params;

        internal class ParamMap
        {
            public string msg;
        }
    }

    /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum ClientActionType
    {
        runcmdrequest,
        broadcast,
        tellraw
    }

    internal abstract class ClientPackBase : OriginalPack
    {
        [JsonProperty(Order = -2)]
        public abstract ClientActionType action { get; }
    }

    internal class ActionRunCmd : ClientPackBase
    {
        // Friend Sub New(json As JObject)
        // params = GetParams(Of ParamMap)(json)
        // End Sub


        internal ActionRunCmd(JObject json, object con)
        {
            @params = GetParams<ParamMap>(json);
            @params.con = con;
        }

        internal ActionRunCmd(string cmd, string id, object con)
        {
            @params = new ParamMap() { cmd = cmd, id = id, con = con };
        }
        public override ClientActionType action { get; } = ClientActionType.runcmdrequest;
        public ParamMap @params;

        internal class ParamMap
        {
            public string cmd, id;
            [JsonIgnore]
            internal object con;
        }

        internal CauseRuncmdFeedback GetFeedback()
        {
            return new CauseRuncmdFeedback(@params.id, @params.cmd, null, @params.con);
        }
    }
    /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
}