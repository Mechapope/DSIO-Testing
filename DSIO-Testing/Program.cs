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
                            if (cell.Def.Name == "guardAtkRate" || cell.Def.Name == "IgnoreNotifyMissSwingForAI" || cell.Def.Name == "knockbackDist" || cell.Def.Name == "hitStopTime")
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
                            //double hitbox radius?
                            else if (cell.Def.Name == "hit0_Radius")
                            {
                                PropertyInfo prop = cell.GetType().GetProperty("Value");
                                prop.SetValue(cell, (float)(prop.GetValue(cell, null))*2, null);
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
                    List<int> allSPeffects = new List<int>();

                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            //spEffect0-7 rando
                            if (cell.Def.Name.StartsWith("spEffectID"))
                            {
                                PropertyInfo pi = cell.GetType().GetProperty("Value");
                                allSPeffects.Add((int)(pi.GetValue(cell, null)));
                            }



                            if (cell.Def.Name == "turnVellocity")
                            {
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");
                                prop.SetValue(cell, cell.Def.Max, null);
                            }
                        }
                    }

                    //loop again to set a random value per entry
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        Random r = new Random();

                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            if (cell.Def.Name.StartsWith("spEffectID"))
                            {
                                int randomIndex = r.Next(allSPeffects.Count);
                                Type type = cell.GetType();
                                PropertyInfo prop = type.GetProperty("Value");

                                prop.SetValue(cell, allSPeffects[randomIndex], null);

                                allSPeffects.RemoveAt(randomIndex);
                            }
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
                            string[] attrsToMax = { "farDist", "outDist", "eye_dist", "ear_dist", "nose_dist", "maxBackhomeDist", "backhomeDist", "backhomeBattleDist", "BackHome_LookTargetTime", "BackHome_LookTargetDist", "BattleStartDist", "eye_angX", "eye_angY", "ear_angX", "ear_angY", "enableNaviFlg_InSideWall" };
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
                            string[] attrsToMax = { "hierarchyGain", "accelGain", "velocityGain", "positionGain", "maxLinerVelocity", "maxAngularVelocity", "enable" };
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
                    //loop through all entries once to get list of values
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

                    //loop again to set a random value per entry
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
                else if (paramFile.ID == "BULLET_PARAM_ST")
                {
                    //probably wont even use these arrays, just for me to keep track of what im doin

                    //set these props to
                    string[] propsToSwap = { "atkId_Bullet", "sfxId_Bullet", "sfxId_Hit", "sfxId_Flick", "life", "dist", "shootInterval", "gravityInRange", "gravityOutRange", "hormingStopRange",
                        "initVellocity", "accelInRange", "accelOutRange", "maxVellocity", "minVellocity", "accelTime", "homingBeginDist", "hitRadius", "hitRadiusMax", "spreadTime",
                        "hormingOffsetRange", "dmgHitRecordLifeTime", "spEffectIDForShooter", "HitBulletID", "spEffectId0", "numShoot", "homingAngle", "shootAngle", "shootAngleInterval",
                        "shootAngleXInterval", "damageDamp", "spelDamageDamp", "fireDamageDamp", "thunderDamageDamp", "staminaDamp", "knockbackDamp", "shootAngleXZ", "lockShootLimitAng",
                        "isPenetrate", "atkAttribute", "spAttribute", "Material_AttackType", "Material_AttackMaterial", "Material_Size", "launchConditionType", "FollowType", "isAttackSFX",
                        "isEndlessHit", "isPenetrateMap", "isHitBothTeam", "isUseSharedHitList", "isHitForceMagic", "isIgnoreSfxIfHitWater", "IsIgnoreMoveStateIfHitWater", "isHitDarkForceMagic"
                    };

                    List<int> atkId_BulletVals = new List<int>();
                    List<int> sfxId_BulletVals = new List<int>();
                    List<int> sfxId_HitVals = new List<int>();
                    List<int> sfxId_FlickVals = new List<int>();
                    List<float> lifeVals = new List<float>();
                    List<float> distVals = new List<float>();
                    List<float> shootIntervalVals = new List<float>();
                    List<float> gravityInRangeVals = new List<float>();
                    List<float> gravityOutRangeVals = new List<float>();
                    List<float> hormingStopRangeVals = new List<float>();
                    List<float> initVellocityVals = new List<float>();
                    List<float> accelInRangeVals = new List<float>();
                    List<float> accelOutRangeVals = new List<float>();
                    List<float> maxVellocityVals = new List<float>();
                    List<float> minVellocityVals = new List<float>();
                    List<float> accelTimeVals = new List<float>();
                    List<float> homingBeginDistVals = new List<float>();
                    List<float> hitRadiusVals = new List<float>();
                    List<float> hitRadiusMaxVals = new List<float>();
                    List<float> spreadTimeVals = new List<float>();
                    List<float> hormingOffsetRangeVals = new List<float>();
                    List<float> dmgHitRecordLifeTimeVals = new List<float>();
                    List<int> spEffectIDForShooterVals = new List<int>();
                    List<int> HitBulletIDVals = new List<int>();
                    List<int> spEffectId0Vals = new List<int>();
                    List<ushort> numShootVals = new List<ushort>();
                    List<short> homingAngleVals = new List<short>();
                    List<short> shootAngleVals = new List<short>();
                    List<short> shootAngleIntervalVals = new List<short>();
                    List<short> shootAngleXIntervalVals = new List<short>();
                    List<sbyte> damageDampVals = new List<sbyte>();
                    List<sbyte> spelDamageDampVals = new List<sbyte>();
                    List<sbyte> fireDamageDampVals = new List<sbyte>();
                    List<sbyte> thunderDamageDampVals = new List<sbyte>();
                    List<sbyte> staminaDampVals = new List<sbyte>();
                    List<sbyte> knockbackDampVals = new List<sbyte>();
                    List<sbyte> shootAngleXZVals = new List<sbyte>();
                    List<int> lockShootLimitAngVals = new List<int>();
                    List<int> isPenetrateVals = new List<int>();
                    List<int> atkAttributeVals = new List<int>();
                    List<int> spAttributeVals = new List<int>();
                    List<int> Material_AttackTypeVals = new List<int>();
                    List<int> Material_AttackMaterialVals = new List<int>();
                    List<int> Material_SizeVals = new List<int>();
                    List<int> launchConditionTypeVals = new List<int>();
                    List<int> FollowTypeVals = new List<int>();
                    List<int> isAttackSFXVals = new List<int>();
                    List<int> isEndlessHitVals = new List<int>();
                    List<int> isPenetrateMapVals = new List<int>();
                    List<int> isHitBothTeamVals = new List<int>();
                    List<int> isUseSharedHitListVals = new List<int>();
                    List<int> isHitForceMagicVals = new List<int>();
                    List<int> isIgnoreSfxIfHitWaterVals = new List<int>();
                    List<int> IsIgnoreMoveStateIfHitWaterVals = new List<int>();
                    List<int> isHitDarkForceMagicVals = new List<int>();                        


                    //add to list to rando
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            if (cell.Def.Name == "atkId_Bullet")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                atkId_BulletVals.Add((int)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "sfxId_Bullet")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                sfxId_BulletVals.Add((int)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "sfxId_Hit")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                sfxId_HitVals.Add((int)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "sfxId_Flick")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                sfxId_FlickVals.Add((int)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "life")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                lifeVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "dist")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                distVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "shootInterval")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                shootIntervalVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "gravityInRange")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                gravityInRangeVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "gravityOutRange")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                gravityOutRangeVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "hormingStopRange")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                hormingStopRangeVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "initVellocity")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                initVellocityVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "accelInRange")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                accelInRangeVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "accelOutRange")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                accelOutRangeVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "maxVellocity")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                maxVellocityVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "minVellocity")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                minVellocityVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "accelTime")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                accelTimeVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "homingBeginDist")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                homingBeginDistVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "hitRadius")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                hitRadiusVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "hitRadiusMax")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                hitRadiusMaxVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "spreadTime")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                spreadTimeVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "hormingOffsetRange")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                hormingOffsetRangeVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "dmgHitRecordLifeTime")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                dmgHitRecordLifeTimeVals.Add((float)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "spEffectIDForShooter")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                spEffectIDForShooterVals.Add((int)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "HitBulletID")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                HitBulletIDVals.Add((int)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "spEffectId0")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                spEffectId0Vals.Add((int)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "numShoot")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                numShootVals.Add((ushort)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "homingAngle")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                homingAngleVals.Add((short)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "shootAngle")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                shootAngleVals.Add((short)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "shootAngleInterval")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                shootAngleIntervalVals.Add((short)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "shootAngleXInterval")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                shootAngleXIntervalVals.Add((short)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "damageDamp")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                damageDampVals.Add((sbyte)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "spelDamageDamp")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                spelDamageDampVals.Add((sbyte)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "fireDamageDamp")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                fireDamageDampVals.Add((sbyte)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "thunderDamageDamp")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                thunderDamageDampVals.Add((sbyte)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "staminaDamp")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                staminaDampVals.Add((sbyte)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "knockbackDamp")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                knockbackDampVals.Add((sbyte)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "shootAngleXZ")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                shootAngleXZVals.Add((sbyte)(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "lockShootLimitAng")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                lockShootLimitAngVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "isPenetrate")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                isPenetrateVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "atkAttribute")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                atkAttributeVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "spAttribute")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                spAttributeVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "Material_AttackType")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                Material_AttackTypeVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "Material_AttackMaterial")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                Material_AttackMaterialVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "Material_Size")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                Material_SizeVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "launchConditionType")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                launchConditionTypeVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "FollowType")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                FollowTypeVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "isAttackSFX")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                isAttackSFXVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "isEndlessHit")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                isEndlessHitVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "isPenetrateMap")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                isPenetrateMapVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "isHitBothTeam")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                isHitBothTeamVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "isUseSharedHitList")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                isUseSharedHitListVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "isHitForceMagic")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                isHitForceMagicVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "isIgnoreSfxIfHitWater")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                isIgnoreSfxIfHitWaterVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "IsIgnoreMoveStateIfHitWater")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                IsIgnoreMoveStateIfHitWaterVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }                            else if (cell.Def.Name == "isHitDarkForceMagic")                            {                                PropertyInfo pi = cell.GetType().GetProperty("Value");                                isHitDarkForceMagicVals.Add(Convert.ToInt32(pi.GetValue(cell, null)));                            }
                        }
                    }

                    Random r = new Random();
                    //pick from list and remove
                    foreach (MeowDSIO.DataTypes.PARAM.ParamRow paramRow in paramFile.Entries)
                    {
                        foreach (MeowDSIO.DataTypes.PARAM.ParamCellValueRef cell in paramRow.Cells)
                        {
                            if (cell.Def.Name == "atkId_Bullet")                            {                                int randomIndex = r.Next(atkId_BulletVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, atkId_BulletVals[randomIndex], null);                                atkId_BulletVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "sfxId_Bullet")                            {                                int randomIndex = r.Next(sfxId_BulletVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, sfxId_BulletVals[randomIndex], null);                                sfxId_BulletVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "sfxId_Hit")                            {                                int randomIndex = r.Next(sfxId_HitVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, sfxId_HitVals[randomIndex], null);                                sfxId_HitVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "sfxId_Flick")                            {                                int randomIndex = r.Next(sfxId_FlickVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, sfxId_FlickVals[randomIndex], null);                                sfxId_FlickVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "life")                            {                                int randomIndex = r.Next(lifeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, lifeVals[randomIndex], null);                                lifeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "dist")                            {                                int randomIndex = r.Next(distVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, distVals[randomIndex], null);                                distVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "shootInterval")                            {                                int randomIndex = r.Next(shootIntervalVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, shootIntervalVals[randomIndex], null);                                shootIntervalVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "gravityInRange")                            {                                int randomIndex = r.Next(gravityInRangeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, gravityInRangeVals[randomIndex], null);                                gravityInRangeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "gravityOutRange")                            {                                int randomIndex = r.Next(gravityOutRangeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, gravityOutRangeVals[randomIndex], null);                                gravityOutRangeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "hormingStopRange")                            {                                int randomIndex = r.Next(hormingStopRangeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, hormingStopRangeVals[randomIndex], null);                                hormingStopRangeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "initVellocity")                            {                                int randomIndex = r.Next(initVellocityVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, initVellocityVals[randomIndex], null);                                initVellocityVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "accelInRange")                            {                                int randomIndex = r.Next(accelInRangeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, accelInRangeVals[randomIndex], null);                                accelInRangeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "accelOutRange")                            {                                int randomIndex = r.Next(accelOutRangeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, accelOutRangeVals[randomIndex], null);                                accelOutRangeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "maxVellocity")                            {                                int randomIndex = r.Next(maxVellocityVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, maxVellocityVals[randomIndex], null);                                maxVellocityVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "minVellocity")                            {                                int randomIndex = r.Next(minVellocityVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, minVellocityVals[randomIndex], null);                                minVellocityVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "accelTime")                            {                                int randomIndex = r.Next(accelTimeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, accelTimeVals[randomIndex], null);                                accelTimeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "homingBeginDist")                            {                                int randomIndex = r.Next(homingBeginDistVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, homingBeginDistVals[randomIndex], null);                                homingBeginDistVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "hitRadius")                            {                                int randomIndex = r.Next(hitRadiusVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, hitRadiusVals[randomIndex], null);                                hitRadiusVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "hitRadiusMax")                            {                                int randomIndex = r.Next(hitRadiusMaxVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, hitRadiusMaxVals[randomIndex], null);                                hitRadiusMaxVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "spreadTime")                            {                                int randomIndex = r.Next(spreadTimeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, spreadTimeVals[randomIndex], null);                                spreadTimeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "hormingOffsetRange")                            {                                int randomIndex = r.Next(hormingOffsetRangeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, hormingOffsetRangeVals[randomIndex], null);                                hormingOffsetRangeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "dmgHitRecordLifeTime")                            {                                int randomIndex = r.Next(dmgHitRecordLifeTimeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, dmgHitRecordLifeTimeVals[randomIndex], null);                                dmgHitRecordLifeTimeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "spEffectIDForShooter")                            {                                int randomIndex = r.Next(spEffectIDForShooterVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, spEffectIDForShooterVals[randomIndex], null);                                spEffectIDForShooterVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "HitBulletID")                            {                                int randomIndex = r.Next(HitBulletIDVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, HitBulletIDVals[randomIndex], null);                                HitBulletIDVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "spEffectId0")                            {                                int randomIndex = r.Next(spEffectId0Vals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, spEffectId0Vals[randomIndex], null);                                spEffectId0Vals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "numShoot")                            {                                int randomIndex = r.Next(numShootVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, numShootVals[randomIndex], null);                                numShootVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "homingAngle")                            {                                int randomIndex = r.Next(homingAngleVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, homingAngleVals[randomIndex], null);                                homingAngleVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "shootAngle")                            {                                int randomIndex = r.Next(shootAngleVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, shootAngleVals[randomIndex], null);                                shootAngleVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "shootAngleInterval")                            {                                int randomIndex = r.Next(shootAngleIntervalVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, shootAngleIntervalVals[randomIndex], null);                                shootAngleIntervalVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "shootAngleXInterval")                            {                                int randomIndex = r.Next(shootAngleXIntervalVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, shootAngleXIntervalVals[randomIndex], null);                                shootAngleXIntervalVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "damageDamp")                            {                                int randomIndex = r.Next(damageDampVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, damageDampVals[randomIndex], null);                                damageDampVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "spelDamageDamp")                            {                                int randomIndex = r.Next(spelDamageDampVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, spelDamageDampVals[randomIndex], null);                                spelDamageDampVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "fireDamageDamp")                            {                                int randomIndex = r.Next(fireDamageDampVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, fireDamageDampVals[randomIndex], null);                                fireDamageDampVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "thunderDamageDamp")                            {                                int randomIndex = r.Next(thunderDamageDampVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, thunderDamageDampVals[randomIndex], null);                                thunderDamageDampVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "staminaDamp")                            {                                int randomIndex = r.Next(staminaDampVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, staminaDampVals[randomIndex], null);                                staminaDampVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "knockbackDamp")                            {                                int randomIndex = r.Next(knockbackDampVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, knockbackDampVals[randomIndex], null);                                knockbackDampVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "shootAngleXZ")                            {                                int randomIndex = r.Next(shootAngleXZVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, shootAngleXZVals[randomIndex], null);                                shootAngleXZVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "lockShootLimitAng")                            {                                int randomIndex = r.Next(lockShootLimitAngVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, lockShootLimitAngVals[randomIndex], null);                                lockShootLimitAngVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "isPenetrate")                            {                                int randomIndex = r.Next(isPenetrateVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, isPenetrateVals[randomIndex], null);                                isPenetrateVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "atkAttribute")                            {                                int randomIndex = r.Next(atkAttributeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, atkAttributeVals[randomIndex], null);                                atkAttributeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "spAttribute")                            {                                int randomIndex = r.Next(spAttributeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, spAttributeVals[randomIndex], null);                                spAttributeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "Material_AttackType")                            {                                int randomIndex = r.Next(Material_AttackTypeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, Material_AttackTypeVals[randomIndex], null);                                Material_AttackTypeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "Material_AttackMaterial")                            {                                int randomIndex = r.Next(Material_AttackMaterialVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, Material_AttackMaterialVals[randomIndex], null);                                Material_AttackMaterialVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "Material_Size")                            {                                int randomIndex = r.Next(Material_SizeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, Material_SizeVals[randomIndex], null);                                Material_SizeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "launchConditionType")                            {                                int randomIndex = r.Next(launchConditionTypeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, launchConditionTypeVals[randomIndex], null);                                launchConditionTypeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "FollowType")                            {                                int randomIndex = r.Next(FollowTypeVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, FollowTypeVals[randomIndex], null);                                FollowTypeVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "isAttackSFX")                            {                                int randomIndex = r.Next(isAttackSFXVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, isAttackSFXVals[randomIndex], null);                                isAttackSFXVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "isEndlessHit")                            {                                int randomIndex = r.Next(isEndlessHitVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, isEndlessHitVals[randomIndex], null);                                isEndlessHitVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "isPenetrateMap")                            {                                int randomIndex = r.Next(isPenetrateMapVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, isPenetrateMapVals[randomIndex], null);                                isPenetrateMapVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "isHitBothTeam")                            {                                int randomIndex = r.Next(isHitBothTeamVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, isHitBothTeamVals[randomIndex], null);                                isHitBothTeamVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "isUseSharedHitList")                            {                                int randomIndex = r.Next(isUseSharedHitListVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, isUseSharedHitListVals[randomIndex], null);                                isUseSharedHitListVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "isHitForceMagic")                            {                                int randomIndex = r.Next(isHitForceMagicVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, isHitForceMagicVals[randomIndex], null);                                isHitForceMagicVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "isIgnoreSfxIfHitWater")                            {                                int randomIndex = r.Next(isIgnoreSfxIfHitWaterVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, isIgnoreSfxIfHitWaterVals[randomIndex], null);                                isIgnoreSfxIfHitWaterVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "IsIgnoreMoveStateIfHitWater")                            {                                int randomIndex = r.Next(IsIgnoreMoveStateIfHitWaterVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, IsIgnoreMoveStateIfHitWaterVals[randomIndex], null);                                IsIgnoreMoveStateIfHitWaterVals.RemoveAt(randomIndex);                            }                            else if (cell.Def.Name == "isHitDarkForceMagicList")                            {                                int randomIndex = r.Next(isHitDarkForceMagicVals.Count);                                Type type = cell.GetType();                                PropertyInfo prop = type.GetProperty("Value");                                prop.SetValue(cell, isHitDarkForceMagicVals[randomIndex], null);                                isHitDarkForceMagicVals.RemoveAt(randomIndex);                            }
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
