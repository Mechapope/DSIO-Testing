using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeowDSIO;
using MeowDSIO.DataFiles;
using MeowDSIO.DataTypes;
using System.IO;
using System.Collections.ObjectModel;
using System.Reflection;

namespace DSIO_Testing
{

    class Program
    {
        static void Main(string[] args)
        {
            var gameFolder = @"C:\Users\mcouture\Desktop\DarkSoulsAssetRandomizer\DS1MusicMod\DATA\";

            var gameparamBnds = Directory.GetFiles(gameFolder + "param\\GameParam\\", "*.parambnd")
                .Select(p => DataFile.LoadFromFile<BND>(p, new Progress<(int, int) > ((pr) =>
                {

                })));

            var drawparamBnds = Directory.GetFiles(gameFolder + "param\\DrawParam\\", "*.parambnd")
                .Select(p => DataFile.LoadFromFile<BND>(p, new Progress<(int, int) > ((pr) =>
                {

                })));

            List<BND> PARAMBNDs = gameparamBnds.Concat(drawparamBnds).ToList();

            var paramdefBnds = Directory.GetFiles(gameFolder + "paramdef\\", "*.paramdefbnd")
                .Select(p => DataFile.LoadFromFile<BND>(p, new Progress<(int, int) > ((pr) =>
                {

                }))).ToList();

            List<PARAMDEF> ParamDefs = new List<PARAMDEF>();
            List<PARAM> AllParams = new List<PARAM>();

            for (int i = 0; i < paramdefBnds.Count(); i++)
            {
                foreach (var paramdef in paramdefBnds[i])
                {
                    PARAMDEF newParamDef = paramdef.ReadDataAs<PARAMDEF>(new Progress<(int, int) > ((r) =>
                    {

                    }));
                    ParamDefs.Add(newParamDef);
                }
            }                               

            for (int i = 0; i < PARAMBNDs.Count(); i++)
            {
                foreach (var param in PARAMBNDs[i])
                {
                    PARAM newParam = param.ReadDataAs<PARAM>(new Progress<(int, int) > ((p) =>
                    {

                    }));

                    newParam.ApplyPARAMDEFTemplate(ParamDefs.Where(x => x.ID == newParam.ID).First());

                    AllParams.Add(newParam);
                }
            }

            //changing a specific property in a specific param
            //this example is just replacing all Weapon weights with 1
            var paramyboi = AllParams.First(p => p.ID == "EQUIP_PARAM_WEAPON_ST");

            //loop through each row (ID, Name, Cells)
            foreach (MeowDSIO.DataTypes.PARAM.ParamRow row in paramyboi.Entries)
            {
                foreach (var item in row.Cells)
                {
                    if (item.Def.Name == "weight")
                    {
                        //sets the property 'Value' to 1
                        Type type = item.GetType();
                        PropertyInfo prop = type.GetProperty("Value");
                        prop.SetValue(item, (float)1, null);
                        break;
                    }
                }
                //I dont know if this is necessary
                row.ReInitRawData(paramyboi);
                row.SaveValuesToRawData(paramyboi);
            }

            //Resave BNDs
            foreach (var paramBnd in PARAMBNDs)
            {
                foreach (var param in paramBnd)
                {
                    var filteredParamName = param.Name.Substring(param.Name.LastIndexOf("\\") + 1).Replace(".param", "");

                    var matchingParam = AllParams.Where(x => x.VirtualUri == param.Name).First();

                    param.ReplaceData(matchingParam,
                        new Progress<(int, int) > ((p) =>
                        {

                        }));
                }

                DataFile.Resave(paramBnd, new Progress<(int, int) > ((p) =>
                {

                }));
            }

        }
    }
}
