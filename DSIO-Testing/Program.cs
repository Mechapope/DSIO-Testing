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
            List<PARAMDEF> ParamDefs = new List<PARAMDEF>();
            List<PARAM> AllParams = new List<PARAM>();
            var gameFolder = @"C:\Users\mcouture\Desktop\DS-Modding\Dark Souls Prepare to Die Edition\DATA\";

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
            //Loading params complete



            //change all the things
            foreach (PARAM paramFile in AllParams)
            {
                if (paramFile.ID == "AI_STANDARD_INFO_BANK")
                {
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            if (cell.Def.Name == "TerritorySize" || cell.Def.Name == "RadarRange")
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, cell.Def.Max, null);
                            }
                        }
                    }
                }
                //ID is same between PC and NPC atk params - use virtual uri to differentiate
                else if (paramFile.VirtualUri.EndsWith("AtkParam_Npc.param"))
                {
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            if (cell.Def.Name == "guardAtkRate" || cell.Def.Name == "IgnoreNotifyMissSwingForAI", || cell.Def.Name == "knockbackDist")
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, cell.Def.Max, null);
                            }
                            else if (cell.Def.Name == "dmgLevel")
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, 7, null);
                            }
                        }
                    }
                }
                else if (paramFile.VirtualUri.EndsWith("BehaviorParam.param"))
                {
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {

                        }
                    }
                }
                else if (paramFile.ID == "NPC_PARAM_ST")
                {
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {

                        }
                    }
                }
                else if (paramFile.ID == "NPC_THINK_PARAM_ST")
                {
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            //more concise way of listing many cells
                            string[] attrsToMax = { "farDist", "outDist", "eye_dist", "ear_dist", "nose_dist", "maxBackhomeDist", "backhomeDist", "backhomeBattleDist", "BackHome_LookTargetTime", "BackHome_LookTargetDist", "BattleStartDist", "eye_angX", "eye_angY", "ear_angX", "ear_angY", };
                            if (attrsToMax.Contains(cell.Def.Name))
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, cell.Def.Max, null);
                            }
                        }
                    }
                }
                else if (paramFile.ID == "OBJECT_PARAM_ST")
                {
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            //if (cell.Def.Name == "isLadder")
                            //if (cell.Def.Name == "breakOnPlayerCollide")
                            if (cell.Def.Name == "isMoveObj")
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, 1, null);
                            }
                        }
                    }
                }
                else if (paramFile.ID == "RAGDOLL_PARAM_ST")
                {
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            string[] attrsToMax = { "hierarchyGain", "accelGain", "velocityGain", "positionGain", "maxLinerVelocity", "maxAngularVelocity" };
                            if (attrsToMax.Contains(cell.Def.Name))
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, cell.Def.Max, null);
                            }
                        }
                    }
                }
                else if (paramFile.ID == "SKELETON_PARAM_ST")
                {
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            string[] attrsToMax = { "maxAnkleHeightMS", "cosineMaxKneeAngle", "neckTurnMaxAngle", "groundAscendingGain", "groundDescendingGain", "footRaisedGain" };
                            if (attrsToMax.Contains(cell.Def.Name))
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, cell.Def.Max, null);
                            }

                            string[] attrsToMin = { "minAnkleHeightMS", "cosineMinKneeAngle" };
                            if (attrsToMin.Contains(cell.Def.Name))
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, cell.Def.Min, null);
                            }
                        }
                    }
                }
                else if (paramFile.ID == "TALK_PARAM_ST")
                {
                    List<int> allSounds = new List<int>();
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            if (cell.Def.Name == "voiceId")
                            {
                                PropertyInfo pi = cell.GetType().GetProperty("Value");
                                allSounds.Add((int)(pi.GetValue(cell, null)));
                            }
                        }
                    }

                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        Random r = new Random();                       
                        
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            if (cell.Def.Name == "voiceId")
                            {
                                int randomIndex = r.Next(allSounds.Count);
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");

                                prop.SetValue(cell, allSounds[randomIndex], null);

                                allSounds.RemoveAt(randomIndex);
                            }
                        }
                    }
                }
                else if (paramFile.ID == "THROW_INFO_BANK")
                {
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            if (cell.Def.Name == "Dist" || cell.Def.Name == "diffAngMax" || cell.Def.Name == "upperYRange" || cell.Def.Name == "lowerYRange")
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, cell.Def.Max, null);
                            }
                        }
                    }
                }
            }


            /*
             * 
             * this was my testing that i'm keep for reference
             * 
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
                        prop.SetValue(item, (float)2, null);
                        break;
                    }
                }
                //I dont know if this is necessary
                //row.ReInitRawData(paramyboi);
                //row.SaveValuesToRawData(paramyboi);
                //according to testing, its not
            }
            */

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
