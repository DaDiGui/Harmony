using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    [PublicizedFrom(EAccessModifier.Private)]
    public enum CloudTypes
    {
        Whispy,
        Fluffy,
        ThickOvercast
    }

    public class ParticleData
    {
        public BiomeWeather weather;

        public int stormLevel;

        public float intensity = -1f;

        public float thunderDelay;

        public GameObject rootObj;

        public Transform rootT;

        public Material rootParticleMat;

        public ParticleSystem rootParticleSys;

        public float rootEmissionMaxRate = 1f;

        public ParticleSystem nearParticleSys;

        public float nearEmissionMaxRate = 1f;

        public ParticleSystem topParticleSys;

        public float topEmissionMaxRate = 1f;

        public Transform farT;

        public ParticleSystem farParticleSys;

        public Color farBaseColor;

        public ParticleSystem ringParticleSys;

        public float ringRadiusMax;

        public List<ParticleSystem> particleSystems = new List<ParticleSystem>();

        public float[] psEmissionMaxRates;

        public Transform forceT;
    }

    [Serializable]
    public class Param
    {
        public string name;

        public float value;

        public float target;

        public float step1Time;

        [NonSerialized]
        [PublicizedFrom(EAccessModifier.Private)]
        public float lastTime;

        public Param(float _value, float _step1Time = 0.25f)
        {
            name = "Param";
            value = _value;
            target = _value;
            step1Time = _step1Time;
        }

        public void FrameUpdate()
        {
            float time = Time.time;
            if (value == target)
            {
                lastTime = time;
                return;
            }

            float num = time - lastTime;
            if (num < 0f)
            {
                lastTime = time;
            }

            if (!(num >= 0.01f))
            {
                return;
            }

            if (num > 1f)
            {
                num = 1f;
            }

            float num2 = num / step1Time;
            if (value > target)
            {
                value -= num2;
                if (value < target)
                {
                    value = target;
                }
            }
            else
            {
                value += num2;
                if (value > target)
                {
                    value = target;
                }
            }

            lastTime = time;
        }

        public void Clamp()
        {
            value = Utils.FastClamp01(value);
            target = Utils.FastClamp01(target);
        }

        public void Set(float _value)
        {
            value = _value;
            target = _value;
        }

        public void SetTarget(float _target)
        {
            if (target != _target)
            {
                target = _target;
                lastTime = Time.time;
            }
        }
    }

    [Serializable]
    public class BiomeWeather
    {
        [NonSerialized]
        [PublicizedFrom(EAccessModifier.Private)]
        public const float cStep1Time = 0.15f;

        [NonSerialized]
        [PublicizedFrom(EAccessModifier.Private)]
        public const float cPrecipitationPercentVisibleMin = 0.3f;

        public BiomeDefinition biomeDefinition;

        public int stormWorldTime;

        public int stormDuration;

        public int stormState;

        public int nextRandWorldTime;

        public byte remainingSeconds;

        public Param[] parameters = new Param[5];

        public float[] parameterFinals = new float[5];

        public Param rainParam = new Param(0f);

        public Param snowFallParam = new Param(0f);

        public BiomeWeather(BiomeDefinition _definition)
        {
            biomeDefinition = _definition;
            for (int i = 0; i < 5; i++)
            {
                parameters[i] = new Param(0f, 0.3f);
            }

            parameters[0].step1Time = 0.15f;
        }

        public float CloudThickness()
        {
            return parameters[2].value;
        }

        public float FogPercent()
        {
            return parameters[4].value * 0.01f;
        }

        public float Wind()
        {
            return parameters[3].value;
        }

        public void FrameUpdate(bool _isServer)
        {
            Param[] array = parameters;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].FrameUpdate();
            }

            float value = parameters[1].value;
            float num = parameters[2].value;
            float _temperature = parameters[0].value;
            CalcGlobalTemperature(value, num, ref _temperature);
            if (_isServer)
            {
                if (forceClouds >= 0f)
                {
                    num = forceClouds * 100f;
                }

                if (forceTemperature > -100f)
                {
                    _temperature = forceTemperature;
                }
            }

            parameterFinals[1] = value;
            parameterFinals[2] = num;
            parameterFinals[0] = _temperature;
            parameterFinals[3] = parameters[3].value;
            parameterFinals[4] = parameters[4].value;
            float v = (value * 0.01f - 0.3f) / 0.7f;
            v = Utils.FastMax(0f, v);
            rainParam.target = ((_temperature > 32f) ? v : 0f);
            snowFallParam.target = ((_temperature <= 32f) ? v : 0f);
            rainParam.FrameUpdate();
            snowFallParam.FrameUpdate();
        }

        [PublicizedFrom(EAccessModifier.Private)]
        public void CalcGlobalTemperature(float _precipitation, float _cloudThickness, ref float _temperature)
        {
            float num = Utils.FastClamp(_precipitation, 0f, 100f);
            _temperature -= num * 0.1f;
            float num2 = SkyManager.GetSunPercent();
            if (num2 > 0f)
            {
                num2 *= 1f - _cloudThickness * 0.01f;
            }

            _temperature += num2 * 10f;
        }

        public void ParamsFrameUpdate()
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                Param param = parameters[i];
                param.FrameUpdate();
                parameterFinals[i] = param.value;
            }

            rainParam.FrameUpdate();
            snowFallParam.FrameUpdate();
        }

        public void Reset()
        {
            stormWorldTime = 0;
            nextRandWorldTime = 0;
            biomeDefinition.WeatherRandomize(0f);
            for (int i = 0; i < 5; i++)
            {
                parameters[i].Set(biomeDefinition.WeatherGetValue((BiomeDefinition.Probabilities.ProbType)i));
            }

            rainParam.Set(0f);
            snowFallParam.Set(0f);
        }

        public void ServerTimeUpdate(int _worldTime, float _freq)
        {
            if (_freq == 0f)
            {
                stormWorldTime = int.MaxValue;
            }
            else if (stormWorldTime == int.MaxValue)
            {
                stormWorldTime = 0;
            }

            int num = _worldTime - stormWorldTime;
            if (num >= 0)
            {
                int num2 = biomeDefinition.WeatherGetDuration("stormbuild");
                int num3 = num2 - num;
                if (num3 > 0)
                {
                    if (stormState != 1)
                    {
                        stormState = 1;
                        SetWeather("stormbuild");
                    }

                    int @int = GameStats.GetInt(EnumGameStats.TimeOfDayIncPerSec);
                    remainingSeconds = (byte)Utils.FastMin(num3 / @int, 255);
                    return;
                }

                if (worldTime < stormWorldTime + stormDuration)
                {
                    if (stormState != 2)
                    {
                        stormState = 2;
                        SetWeather("storm");
                    }

                    return;
                }

                stormState = 0;
                stormDuration = num2;
                Vector2i _delay;
                int num4 = biomeDefinition.WeatherGetDuration("storm", out _delay);
                stormWorldTime = worldTime + (int)((float)gameRand.RandomRange(_delay.x, _delay.y) / _freq);
                int num5 = num4 / 8;
                stormDuration += gameRand.RandomRange(num4 - num5, num4 + num5);
            }

            if (_worldTime >= nextRandWorldTime)
            {
                float rand = gameRand.RandomFloat;
                if (forceSimRandom >= 0f)
                {
                    rand = forceSimRandom;
                }

                SetWeather(_worldTime, rand);
            }
        }

        [PublicizedFrom(EAccessModifier.Private)]
        public void SetWeather(int _worldTime, float _rand)
        {
            if (biomeDefinition != null)
            {
                biomeDefinition.WeatherRandomize(_rand);
                for (int i = 0; i < 5; i++)
                {
                    BiomeDefinition.Probabilities.ProbType type = (BiomeDefinition.Probabilities.ProbType)i;
                    parameters[i].target = biomeDefinition.WeatherGetValue(type);
                }

                nextRandWorldTime = _worldTime + biomeDefinition.currentWeatherGroup.duration;
            }
        }

        public void SetWeather(string name)
        {
            if (biomeDefinition != null)
            {
                biomeDefinition.WeatherRandomize(name);
                for (int i = 0; i < 5; i++)
                {
                    BiomeDefinition.Probabilities.ProbType type = (BiomeDefinition.Probabilities.ProbType)i;
                    parameters[i].target = biomeDefinition.WeatherGetValue(type);
                }

                nextRandWorldTime = 0;
            }
        }

        public override string ToString()
        {
            string text = $"{biomeDefinition.m_sBiomeName}: {biomeDefinition.weatherName}, nxtT{nextRandWorldTime}, ";
            for (int i = 0; i < 5; i++)
            {
                BiomeDefinition.Probabilities.ProbType probType = (BiomeDefinition.Probabilities.ProbType)i;
                text += $"{probType} {biomeDefinition.WeatherGetValue(probType)}, ";
            }

            text += $"rain {rainParam.value}, ";
            text += $"snow {snowFallParam.value}, ";
            return text + $"storm WT {stormWorldTime}, dur {stormDuration}, state {stormState}";
        }
    }

    public const ushort cVersion = 3;

    public static WeatherManager Instance;

    public const float cStormWarningDuration = 60f;

    public const int BaseTemperature = 70;

    public List<BiomeWeather> biomeWeather;

    public static float forceClouds = -1f;

    public static float forceRain = -1f;

    public static float forceSnowfall = -1f;

    public const float cForceTempDefault = -100f;

    public static float forceTemperature = -100f;

    public static float forceWind = -1f;

    public static bool needToReUpdateWeatherSpectrums;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public static float forceSimRandom = -1f;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public const int cGracePeriodWorldTime = 22000;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public static int worldTime;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public World world;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public static GameRandom gameRand;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public bool isGameModeNormal;

    public static bool inWeatherGracePeriod = true;

    public static BiomeWeather currentWeather;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public WeatherPackage[] weatherPackages;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public const float cLightningDelayMin = 30f;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public const float cLightningDelayMax = 60f;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float thunderDelay = 30f;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public const float cWeatherUpdateFreq = 10f;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public const float cWeatherTransitionSeconds = 10f;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public int frameCount = -1;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public static string[] strCloudTypes = new string[3] { "Whispy", "Fluffy", "ThickOvercast" };

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public static Texture[] clouds = new Texture[strCloudTypes.Length];

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public bool isCurrentWeatherUpdatedFirstTime;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public const float cWindMax = 100f;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public const float cWindScale = 0.01f;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public GameObject windZoneObj;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public WindZone windZone;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float windSpeed;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float windSpeedPrevious;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float windTime;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float windTimePrevious;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float windGust;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float windGustStep;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float windGustTarget;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float windGustTime;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public Camera mainCamera;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public int raycastMask;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public GameObject rainParticleObj;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public Transform rainParticleT;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public ParticleSystem rainParticleSys;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public Material rainParticleMat;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float rainEmissionMaxRate = 1f;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public const float cIntensityOff = -1f;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public ParticleData snowData = new ParticleData();

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public const int cStormTypeCount = 4;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public string[] stormNames = new string[4] { "Burnt", "Desert", "Snow", "Wasteland" };

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public BiomeDefinition.BiomeType[] stormBiomes = new BiomeDefinition.BiomeType[4]
    {
        BiomeDefinition.BiomeType.burnt_forest,
        BiomeDefinition.BiomeType.Desert,
        BiomeDefinition.BiomeType.Snow,
        BiomeDefinition.BiomeType.Wasteland
    };

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public ParticleData[] stormData = new ParticleData[4];

    public float spectrumBlend = 1f;

    public static SpectrumWeatherType forcedSpectrum = SpectrumWeatherType.None;

    public SpectrumWeatherType spectrumSourceType;

    public SpectrumWeatherType spectrumTargetType;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public static AtmosphereEffect[] atmosphereSpectrum;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public Vector3 playerPosition;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float checkPlayerMoveTime;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public static MemoryStream loadData;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public string weatherAllName;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float weatherLastUpdateWorldTime;

    public string CustomWeatherName = "";

    public float CustomWeatherTime = -1f;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float cloudThickness;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public float cloudThicknessTarget;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public Vector3 particleFallLastPos;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public Vector3 particleFallPos;

    [NonSerialized]
    [PublicizedFrom(EAccessModifier.Private)]
    public int processingPackageFrame;

    public static void Init(World _world, GameObject _obj)
    {
        Cleanup();
        Instance = _obj.GetComponent<WeatherManager>();
        Instance.Init(_world);
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void Init(World _world)
    {
        world = _world;
        gameRand = _world.GetGameRandom();
        string @string = GamePrefs.GetString(EnumGamePrefs.GameMode);
        isGameModeNormal = !GameModeEditWorld.TypeName.Equals(@string) && !GameModeCreative.TypeName.Equals(@string);
        InitBiomeWeather();
        currentWeather = new BiomeWeather(world.Biomes.GetBiome(3));
        currentWeather.parameters[2].name = "CloudThickness";
        currentWeather.parameters[4].name = "Fog";
        currentWeather.parameters[1].name = "Precipitation";
        currentWeather.parameters[0].name = "Temperature";
        currentWeather.parameters[3].name = "Wind";
        currentWeather.rainParam.name = "Rain";
        currentWeather.snowFallParam.name = "SnowFall";
        ApplyLoad();
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void InitBiomeWeather()
    {
        this.biomeWeather = new List<BiomeWeather>();
        foreach (KeyValuePair<uint, BiomeDefinition> item in world.Biomes.GetBiomeMap())
        {
            BiomeDefinition value = item.Value;
            if (value.weatherGroups.Count > 0)
            {
                BiomeWeather biomeWeather = new BiomeWeather(value);
                this.biomeWeather.Add(biomeWeather);
                biomeWeather.Reset();
            }
        }

        int count = this.biomeWeather.Count;
        weatherPackages = new WeatherPackage[count];
        for (int i = 0; i < count; i++)
        {
            BiomeWeather biomeWeather2 = this.biomeWeather[i];
            WeatherPackage weatherPackage = new WeatherPackage();
            weatherPackage.biomeId = biomeWeather2.biomeDefinition.m_Id;
            weatherPackages[i] = weatherPackage;
        }
    }

    public static void Cleanup()
    {
        if ((bool)Instance)
        {
            UnityEngine.Object.DestroyImmediate(Instance.gameObject);
            forceClouds = -1f;
            forceRain = -1f;
            forceSnowfall = -1f;
            forceTemperature = -100f;
            forceWind = -1f;
        }
    }

    public static void SetWorldTime(ulong _time)
    {
        if ((bool)Instance)
        {
            int num = (int)_time;
            if (num < worldTime)
            {
                Instance.AdjustTimeRewind();
            }

            worldTime = num;
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void AdjustTimeRewind()
    {
        weatherLastUpdateWorldTime = 0f;
        for (int i = 0; i < biomeWeather.Count; i++)
        {
            BiomeWeather obj = biomeWeather[i];
            obj.stormWorldTime = 0;
            obj.stormDuration = 0;
            obj.nextRandWorldTime = 0;
        }
    }

    public static void Load(IBinaryReaderOrWriter _RW, int _loadSize)
    {
        if (loadData == null)
        {
            loadData = new MemoryStream();
        }

        loadData.SetLength(_loadSize);
        _RW.ReadWrite(loadData.GetBuffer(), 0, _loadSize);
        loadData.Position = 0L;
    }

    public void ApplyLoad()
    {
        if (loadData != null)
        {
            using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false))
            {
                pooledBinaryReader.SetBaseStream(loadData);
                ReadWriteData(pooledBinaryReader, _load: true);
            }

            loadData = null;
        }
    }

    public static void Save(IBinaryReaderOrWriter _RW)
    {
        if (loadData != null)
        {
            _RW.ReadWrite(loadData.GetBuffer(), 0, (int)loadData.Length);
        }
        else if ((bool)Instance)
        {
            Instance.ReadWriteData(_RW, _load: false);
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void ReadWriteData(IBinaryReaderOrWriter _RW, bool _load)
    {
        ushort num = _RW.ReadWrite((ushort)3);
        if (_load)
        {
            if (num < 3)
            {
                VersionReset();
                return;
            }

            int num2 = _RW.ReadWrite((byte)0);
            for (int i = 0; i < num2; i++)
            {
                int id = _RW.ReadWrite((byte)0);
                BiomeWeather biomeWeather = FindBiomeWeather(id);
                if (biomeWeather == null)
                {
                    VersionReset();
                    break;
                }

                int weatherGroup = _RW.ReadWrite((byte)0);
                biomeWeather.biomeDefinition.SetWeatherGroup(weatherGroup);
                biomeWeather.stormWorldTime = _RW.ReadWrite(0);
                biomeWeather.stormDuration = _RW.ReadWrite((short)0);
                biomeWeather.nextRandWorldTime = _RW.ReadWrite(0);
                for (int j = 0; j < 5; j++)
                {
                    float value = _RW.ReadWrite(0f);
                    biomeWeather.parameters[j].Set(value);
                    biomeWeather.biomeDefinition.WeatherSetValue((BiomeDefinition.Probabilities.ProbType)j, value);
                }

                biomeWeather.rainParam.Set(_RW.ReadWrite(0f));
                biomeWeather.snowFallParam.Set(_RW.ReadWrite(0f));
            }

            return;
        }

        byte b = (byte)this.biomeWeather.Count;
        _RW.ReadWrite(b);
        for (int k = 0; k < b; k++)
        {
            BiomeWeather biomeWeather2 = this.biomeWeather[k];
            _RW.ReadWrite(biomeWeather2.biomeDefinition.m_Id);
            _RW.ReadWrite((byte)biomeWeather2.biomeDefinition.currentWeatherGroupIndex);
            _RW.ReadWrite(biomeWeather2.stormWorldTime);
            _RW.ReadWrite((short)biomeWeather2.stormDuration);
            _RW.ReadWrite(biomeWeather2.nextRandWorldTime);
            for (int l = 0; l < 5; l++)
            {
                _RW.ReadWrite(biomeWeather2.parameters[l].value);
            }

            _RW.ReadWrite(biomeWeather2.rainParam.value);
            _RW.ReadWrite(biomeWeather2.snowFallParam.value);
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public BiomeWeather FindBiomeWeather(int _id)
    {
        for (int i = 0; i < this.biomeWeather.Count; i++)
        {
            BiomeWeather biomeWeather = this.biomeWeather[i];
            if (biomeWeather.biomeDefinition.m_Id == _id)
            {
                return biomeWeather;
            }
        }

        return null;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public BiomeWeather FindBiomeWeather(BiomeDefinition.BiomeType _type)
    {
        for (int i = 0; i < this.biomeWeather.Count; i++)
        {
            BiomeWeather biomeWeather = this.biomeWeather[i];
            if (biomeWeather.biomeDefinition.m_BiomeType == _type)
            {
                return biomeWeather;
            }
        }

        return null;
    }

    public bool IsStorming(BiomeDefinition.BiomeType _type)
    {
        BiomeWeather biomeWeather = Instance.FindBiomeWeather(_type);
        if (biomeWeather != null)
        {
            return biomeWeather.stormState >= 2;
        }

        return false;
    }

    public static void ClearTemperatureOffSetHeights()
    {
    }

    public static void AddTemperatureOffSetHeight(float height, float degreesOffset)
    {
    }

    public static float SeaLevel()
    {
        return 0f;
    }

    public static float GetCloudThickness()
    {
        if (forceClouds >= 0f)
        {
            return forceClouds * 100f;
        }

        if (currentWeather == null)
        {
            return 0f;
        }

        return currentWeather.parameters[2].value;
    }

    public static float GetTemperature()
    {
        if (forceTemperature > -100f)
        {
            return forceTemperature;
        }

        if (currentWeather == null)
        {
            return 0f;
        }

        return currentWeather.parameters[0].value;
    }

    public static float GetWindSpeed()
    {
        if (forceWind >= 0f)
        {
            return forceWind;
        }

        if (currentWeather == null)
        {
            return 0f;
        }

        return currentWeather.parameters[3].value;
    }

    public static void EntityAddedToWorld(Entity entity)
    {
    }

    public static void EntityRemovedFromWorld(Entity entity)
    {
    }

    public float GetCurrentSnowfallValue()
    {
        if (!(forceSnowfall >= 0f) && currentWeather != null)
        {
            return currentWeather.snowFallParam.value;
        }

        return forceSnowfall;
    }

    public float GetCurrentRainfallValue()
    {
        if (!(forceRain >= 0f) && currentWeather != null)
        {
            return currentWeather.rainParam.value;
        }

        return forceRain;
    }

    public float GetCurrentCloudThicknessPercent()
    {
        return GetCloudThickness() * 0.01f;
    }

    public float GetCurrentTemperatureValue()
    {
        return GetTemperature();
    }

    public static void SetSimRandom(float _random)
    {
        forceSimRandom = _random;
        if ((bool)Instance)
        {
            Instance.weatherLastUpdateWorldTime = 0f;
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public static void LoadSpectrums()
    {
        if (atmosphereSpectrum == null)
        {
            atmosphereSpectrum = new AtmosphereEffect[Enum.GetNames(typeof(SpectrumWeatherType)).Length - 1];
            ReloadSpectrums();
        }
    }

    public static void ReloadSpectrums()
    {
        atmosphereSpectrum[1] = AtmosphereEffect.Load("Snowy", null);
        atmosphereSpectrum[2] = AtmosphereEffect.Load("Stormy", null);
        atmosphereSpectrum[3] = AtmosphereEffect.Load("Rainy", null);
        atmosphereSpectrum[4] = AtmosphereEffect.Load("Foggy", null);
        atmosphereSpectrum[5] = AtmosphereEffect.Load("BloodMoon", null);
    }

    public void Start()
    {
        windZoneObj = GameObject.Find("WindZone");
        if ((bool)windZoneObj)
        {
            windZone = windZoneObj.GetComponent<WindZone>();
        }

        LoadSpectrums();
        raycastMask = LayerMask.GetMask("Water", "NoShadow", "Items", "CC Physics", "TerrainCollision", "CC Physics Dead", "CC Local Physics") | 1;
        string text = "@:Textures/Environment/Spectrums/default/";
        for (int i = 0; i < strCloudTypes.Length; i++)
        {
            Texture texture = DataLoader.LoadAsset<Texture>(text + strCloudTypes[i] + "Clouds.tga");
            clouds[i] = texture;
        }
    }

    public void PushTransitions()
    {
        spectrumBlend = 1f;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void CurrentWeatherFromNearBiomesFrameUpdate()
    {
        if (world.BiomeAtmosphereEffects == null)
        {
            return;
        }

        BiomeDefinition[] nearBiomes = world.BiomeAtmosphereEffects.nearBiomes;
        BiomeDefinition biomeDefinition = nearBiomes[0];
        currentWeather.biomeDefinition = biomeDefinition;
        if (biomeDefinition == null)
        {
            return;
        }

        for (int i = 0; i < currentWeather.parameters.Length; i++)
        {
            currentWeather.parameters[i].target = 0f;
        }

        inWeatherGracePeriod = (worldTime < 22000 || !isGameModeNormal) && CustomWeatherTime == -1f;
        if (inWeatherGracePeriod)
        {
            currentWeather.rainParam.Set(0f);
            currentWeather.snowFallParam.Set(0f);
            int num = 70;
            switch (biomeDefinition.m_BiomeType)
            {
                case BiomeDefinition.BiomeType.Snow:
                    num = 45;
                    break;
                case BiomeDefinition.BiomeType.Forest:
                case BiomeDefinition.BiomeType.PineForest:
                    num = 60;
                    break;
            }

            currentWeather.parameters[0].Set(num);
            currentWeather.parameters[3].Set(8f);
            return;
        }

        BiomeWeather biomeWeather = FindBiomeWeather(biomeDefinition.m_BiomeType);
        currentWeather.remainingSeconds = biomeWeather.remainingSeconds;
        currentWeather.rainParam.target = biomeWeather.rainParam.value;
        currentWeather.snowFallParam.target = biomeWeather.snowFallParam.value;
        if (!isCurrentWeatherUpdatedFirstTime)
        {
            currentWeather.rainParam.value = biomeWeather.rainParam.value;
            currentWeather.snowFallParam.value = biomeWeather.snowFallParam.value;
        }

        float num2 = 0f;
        foreach (BiomeDefinition biomeDefinition2 in nearBiomes)
        {
            if (biomeDefinition2 != null)
            {
                num2 += biomeDefinition2.currentPlayerIntensity;
            }
        }

        foreach (BiomeDefinition biomeDefinition3 in nearBiomes)
        {
            if (biomeDefinition3 == null)
            {
                continue;
            }

            biomeWeather = FindBiomeWeather(biomeDefinition3.m_BiomeType);
            float num3 = Utils.FastClamp01(biomeDefinition3.currentPlayerIntensity / num2);
            for (int l = 0; l < currentWeather.parameters.Length; l++)
            {
                Param param = currentWeather.parameters[l];
                param.target += biomeWeather.parameterFinals[l] * num3;
                if (!isCurrentWeatherUpdatedFirstTime)
                {
                    param.value = param.target;
                }
            }
        }

        isCurrentWeatherUpdatedFirstTime = true;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void GenerateWeatherServerFrameUpdate()
    {
        if (inWeatherGracePeriod)
        {
            GeneralReset();
            return;
        }

        if (CustomWeatherTime > 0f)
        {
            CustomWeatherTime -= Time.deltaTime;
            if (CustomWeatherTime <= 0f)
            {
                CustomWeatherName = "";
                CustomWeatherTime = -1f;
                GeneralReset();
                SetAllWeather("default");
                weatherLastUpdateWorldTime = worldTime;
            }

            return;
        }

        string text = CalcGlobalWeatherType();
        if (text != null)
        {
            SetAllWeather(text);
            return;
        }

        weatherAllName = null;
        if (Utils.FastAbs((float)worldTime - weatherLastUpdateWorldTime) >= 10f)
        {
            weatherLastUpdateWorldTime = worldTime;
            float freq = (float)GameStats.GetInt(EnumGameStats.StormFreq) * 0.01f;
            for (int i = 0; i < biomeWeather.Count; i++)
            {
                biomeWeather[i].ServerTimeUpdate(worldTime, freq);
            }
        }
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public string CalcGlobalWeatherType()
    {
        if (SkyManager.IsBloodMoonVisible())
        {
            int num = 5000;
            for (int i = 0; i < this.biomeWeather.Count; i++)
            {
                BiomeWeather biomeWeather = this.biomeWeather[i];
                if (biomeWeather.stormWorldTime - worldTime < num)
                {
                    biomeWeather.stormWorldTime = worldTime + num;
                }
            }

            return "bloodMoon";
        }

        return null;
    }

    public void ForceWeather(string _weatherName, float _duration)
    {
        CustomWeatherName = _weatherName;
        CustomWeatherTime = _duration;
        GeneralReset();
        SetAllWeather(_weatherName);
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void SetAllWeather(string _weatherName)
    {
        if (weatherAllName != _weatherName)
        {
            weatherAllName = _weatherName;
            for (int i = 0; i < biomeWeather.Count; i++)
            {
                biomeWeather[i].SetWeather(_weatherName);
            }
        }
    }

    public void SetStorm(string _biomeName, int _duration)
    {
        for (int i = 0; i < this.biomeWeather.Count; i++)
        {
            BiomeWeather biomeWeather = this.biomeWeather[i];
            if (_biomeName == null || biomeWeather.biomeDefinition.m_sBiomeName == _biomeName)
            {
                biomeWeather.stormWorldTime = worldTime;
                biomeWeather.stormDuration = _duration;
            }
        }
    }

    public void TriggerUpdate()
    {
        weatherAllName = null;
        weatherLastUpdateWorldTime = 0f;
    }

    public void VersionReset()
    {
        GeneralReset();
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void GeneralReset()
    {
    }

    public void FrameUpdate()
    {
        if (GameManager.Instance == null || SkyManager.random == null || world == null)
        {
            return;
        }

        float time = Time.time;
        EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
        if (time > checkPlayerMoveTime + 1f)
        {
            checkPlayerMoveTime = time;
            if ((bool)primaryPlayer)
            {
                Vector3 vector = primaryPlayer.position - playerPosition;
                if (vector.x * vector.x + vector.y * vector.y + vector.z * vector.z > 400f)
                {
                    spectrumBlend = 1f;
                }

                playerPosition = primaryPlayer.position;
            }
        }

        int num = Time.frameCount;
        if (frameCount == num)
        {
            return;
        }

        frameCount = num;
        if ((bool)primaryPlayer)
        {
            ParticlesFrameUpdate(primaryPlayer);
            primaryPlayer.WeatherStatusFrameUpdate();
        }

        bool flag = GameManager.IsDedicatedServer || SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
        if (flag)
        {
            GenerateWeatherServerFrameUpdate();
        }

        for (int i = 0; i < biomeWeather.Count; i++)
        {
            biomeWeather[i].FrameUpdate(flag);
        }

        CurrentWeatherFromNearBiomesFrameUpdate();
        currentWeather.ParamsFrameUpdate();
        thunderDelay -= Time.deltaTime;
        if (thunderDelay <= 0f)
        {
            thunderDelay = Utils.FastLerpUnclamped(30f, 60f, SkyManager.random.RandomFloat);
            if (((((forceRain >= 0f) ? forceRain : currentWeather.rainParam.value) > 0.5f && ((forceClouds >= 0f) ? (forceClouds * 100f) : currentWeather.CloudThickness()) >= 70f) || SkyManager.IsBloodMoonVisible()) && EnvironmentAudioManager.Instance != null)
            {
                EnvironmentAudioManager.Instance.TriggerThunder(world.GetPrimaryPlayer().position);
            }
        }

        if (needToReUpdateWeatherSpectrums)
        {
            needToReUpdateWeatherSpectrums = false;
            spectrumBlend = 1f;
        }

        SpectrumsFrameUpdate();
        CloudsFrameUpdate();
        WindFrameUpdate();
        TriggerEffectManager.UpdateDualSenseLightFromWeather(currentWeather);
    }

    public void CloudsFrameUpdateNow()
    {
        CloudsFrameUpdate();
        cloudThickness = cloudThicknessTarget;
    }

    public void CloudsFrameUpdate()
    {
        if (currentWeather == null)
        {
            return;
        }

        float num = currentWeather.CloudThickness();
        if (forceClouds >= 0f)
        {
            num = forceClouds * 100f;
        }

        cloudThicknessTarget = num;
        if (num < 20f)
        {
            cloudThicknessTarget = 0f;
        }

        cloudThickness = Mathf.MoveTowards(cloudThickness, cloudThicknessTarget, 0.05f);
        Texture mainTex;
        Texture blendTex;
        float cloudTransition;
        if (cloudThickness <= 40f)
        {
            mainTex = clouds[0];
            blendTex = clouds[1];
            cloudTransition = cloudThickness / 40f;
        }
        else
        {
            mainTex = clouds[2];
            blendTex = clouds[1];
            cloudTransition = (cloudThickness - 40f) / 50f;
            if (cloudTransition >= 1f)
            {
                cloudTransition = 1f;
            }

            cloudTransition = 1f - cloudTransition;
        }

        SkyManager.SetCloudTextures(mainTex, blendTex);
        SkyManager.SetCloudTransition(cloudTransition);
    }

    public void WindFrameUpdate()
    {
        float deltaTime = Time.deltaTime;
        float num = GetWindSpeed();
        float num2 = num * 0.01f;
        windGust += windGustStep * deltaTime;
        if (windGust <= 0f)
        {
            windGust = 0f;
            windGustTime -= deltaTime;
            if (windGustTime <= 0f)
            {
                GameRandom gameRandom = world.GetGameRandom();
                windGustTarget = (3f + num * 0.33f) * gameRandom.RandomFloat + 5f;
                windGustStep = 0.35f * windGustTarget;
                windGustTime = (1f + 5f * gameRandom.RandomFloat) * (1f - num2) + 0.5f;
            }
        }

        if (windGust > windGustTarget)
        {
            windGust = windGustTarget;
            windGustStep = 0f - windGustStep;
        }

        num += windGust;
        num *= 0.01f;
        windZone.windMain = num * 1.5f;
        windSpeedPrevious = windSpeed;
        windSpeed = num;
        windTimePrevious = windTime;
        windTime += num * deltaTime;
        Shader.SetGlobalVector("_Wind", new Vector4(windSpeed, windTime, windSpeedPrevious, windTimePrevious));
    }

    public void InitParticles()
    {
        //IL_0034: Unknown result type (might be due to invalid IL or missing references)
        //IL_0039: Unknown result type (might be due to invalid IL or missing references)
        //IL_003d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0042: Unknown result type (might be due to invalid IL or missing references)
        GetParticleParts("Rain", out rainParticleObj, out rainParticleMat, out rainParticleSys);
        rainParticleT = rainParticleObj.transform;
        EmissionModule emission = rainParticleSys.emission;
        MinMaxCurve rateOverTime = ((EmissionModule)(ref emission)).rateOverTime;
        rainEmissionMaxRate = ((MinMaxCurve)(ref rateOverTime)).constant;
        InitParticleData("Snow", snowData, _isStorm: false);
        for (int i = 0; i < 4; i++)
        {
            ParticleData particleData = new ParticleData();
            stormData[i] = particleData;
            particleData.weather = FindBiomeWeather(stormBiomes[i]);
            string text = "Storm" + stormNames[i];
            InitParticleData(text, particleData, _isStorm: true);
        }
    }

    public void InitParticleData(string _name, ParticleData _data, bool _isStorm)
    {
        //IL_0030: Unknown result type (might be due to invalid IL or missing references)
        //IL_0035: Unknown result type (might be due to invalid IL or missing references)
        //IL_0039: Unknown result type (might be due to invalid IL or missing references)
        //IL_003e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0077: Unknown result type (might be due to invalid IL or missing references)
        //IL_007c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0080: Unknown result type (might be due to invalid IL or missing references)
        //IL_0085: Unknown result type (might be due to invalid IL or missing references)
        //IL_00be: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c3: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c7: Unknown result type (might be due to invalid IL or missing references)
        //IL_00cc: Unknown result type (might be due to invalid IL or missing references)
        //IL_0130: Unknown result type (might be due to invalid IL or missing references)
        //IL_0135: Unknown result type (might be due to invalid IL or missing references)
        //IL_0139: Unknown result type (might be due to invalid IL or missing references)
        //IL_013e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0176: Unknown result type (might be due to invalid IL or missing references)
        //IL_017b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0206: Unknown result type (might be due to invalid IL or missing references)
        //IL_020b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0216: Unknown result type (might be due to invalid IL or missing references)
        //IL_021b: Unknown result type (might be due to invalid IL or missing references)
        GetParticleParts(_name, out _data.rootObj, out _data.rootParticleMat, out _data.rootParticleSys);
        _data.rootT = _data.rootObj.transform;
        EmissionModule emission = _data.rootParticleSys.emission;
        MinMaxCurve rateOverTime = ((EmissionModule)(ref emission)).rateOverTime;
        _data.rootEmissionMaxRate = ((MinMaxCurve)(ref rateOverTime)).constant;
        Transform transform = _data.rootT.Find("Near");
        if ((bool)transform)
        {
            _data.nearParticleSys = transform.GetComponent<ParticleSystem>();
            emission = _data.nearParticleSys.emission;
            rateOverTime = ((EmissionModule)(ref emission)).rateOverTime;
            _data.nearEmissionMaxRate = ((MinMaxCurve)(ref rateOverTime)).constant;
        }

        Transform transform2 = _data.rootT.Find("Top");
        if ((bool)transform2)
        {
            _data.topParticleSys = transform2.GetComponent<ParticleSystem>();
            emission = _data.topParticleSys.emission;
            rateOverTime = ((EmissionModule)(ref emission)).rateOverTime;
            _data.topEmissionMaxRate = ((MinMaxCurve)(ref rateOverTime)).constant;
        }

        Transform transform3 = _data.rootT.Find("Far");
        if ((bool)transform3)
        {
            if (_isStorm)
            {
                transform3.SetParent(_data.rootT.parent);
                transform3.gameObject.SetActive(value: false);
            }

            _data.farT = transform3;
            _data.farParticleSys = transform3.GetComponent<ParticleSystem>();
            MainModule main = _data.farParticleSys.main;
            MinMaxGradient startColor = ((MainModule)(ref main)).startColor;
            _data.farBaseColor = ((MinMaxGradient)(ref startColor)).color;
            Transform transform4 = transform3.Find("Ring");
            if ((bool)transform4)
            {
                _data.ringParticleSys = transform4.GetComponent<ParticleSystem>();
                ShapeModule shape = _data.ringParticleSys.shape;
                _data.ringRadiusMax = ((ShapeModule)(ref shape)).radius;
            }
        }

        _data.rootObj.GetComponentsInChildren(includeInactive: true, _data.particleSystems);
        for (int num = _data.particleSystems.Count - 1; num >= 0; num--)
        {
            if ((UnityEngine.Object)(object)_data.particleSystems[num] == (UnityEngine.Object)(object)_data.farParticleSys)
            {
                _data.particleSystems.RemoveAt(num);
            }
        }

        _data.psEmissionMaxRates = new float[_data.particleSystems.Count];
        for (int i = 0; i < _data.particleSystems.Count; i++)
        {
            emission = _data.particleSystems[i].emission;
            float[] psEmissionMaxRates = _data.psEmissionMaxRates;
            int num2 = i;
            rateOverTime = ((EmissionModule)(ref emission)).rateOverTime;
            psEmissionMaxRates[num2] = ((MinMaxCurve)(ref rateOverTime)).constant;
        }

        _data.forceT = _data.rootT.Find("Force");
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void GetParticleParts(string name, out GameObject obj, out Material mat, out ParticleSystem ps)
    {
        Transform transform = SkyManager.skyManager.transform.Find(name);
        obj = transform.gameObject;
        ps = obj.GetComponent<ParticleSystem>();
        Renderer component = ((Component)(object)ps).GetComponent<Renderer>();
        mat = component.material;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void ParticlesFrameUpdate(EntityPlayerLocal localPlayer)
    {
        //IL_01e1: Unknown result type (might be due to invalid IL or missing references)
        //IL_01e6: Unknown result type (might be due to invalid IL or missing references)
        //IL_01ff: Unknown result type (might be due to invalid IL or missing references)
        //IL_029b: Unknown result type (might be due to invalid IL or missing references)
        //IL_02a0: Unknown result type (might be due to invalid IL or missing references)
        //IL_02b2: Unknown result type (might be due to invalid IL or missing references)
        //IL_02c7: Unknown result type (might be due to invalid IL or missing references)
        //IL_02cc: Unknown result type (might be due to invalid IL or missing references)
        //IL_02de: Unknown result type (might be due to invalid IL or missing references)
        //IL_02f3: Unknown result type (might be due to invalid IL or missing references)
        //IL_02f8: Unknown result type (might be due to invalid IL or missing references)
        //IL_0315: Unknown result type (might be due to invalid IL or missing references)
        //IL_032a: Unknown result type (might be due to invalid IL or missing references)
        //IL_032f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0333: Unknown result type (might be due to invalid IL or missing references)
        //IL_0338: Unknown result type (might be due to invalid IL or missing references)
        //IL_036b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0559: Unknown result type (might be due to invalid IL or missing references)
        //IL_055e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0562: Unknown result type (might be due to invalid IL or missing references)
        //IL_0567: Unknown result type (might be due to invalid IL or missing references)
        //IL_0596: Unknown result type (might be due to invalid IL or missing references)
        //IL_05b2: Unknown result type (might be due to invalid IL or missing references)
        //IL_05b7: Unknown result type (might be due to invalid IL or missing references)
        //IL_0663: Unknown result type (might be due to invalid IL or missing references)
        //IL_0668: Unknown result type (might be due to invalid IL or missing references)
        //IL_0679: Unknown result type (might be due to invalid IL or missing references)
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        float deltaTime = Time.deltaTime;
        Vector3 position = mainCamera.transform.position;
        position.y += 250f;
        RaycastHit val = default(RaycastHit);
        if (Physics.SphereCast(new Ray(position, Vector3.down), 9f, ref val, float.PositiveInfinity, raycastMask))
        {
            particleFallLastPos = ((RaycastHit)(ref val)).point;
            Vector3 velocityPerSecond = localPlayer.GetVelocityPerSecond();
            particleFallLastPos.x += velocityPerSecond.x * 2f;
            particleFallLastPos.z += velocityPerSecond.z * 2f;
            if (velocityPerSecond.y < -5f)
            {
                velocityPerSecond.y = -5f;
            }

            particleFallLastPos.y += velocityPerSecond.y;
        }

        particleFallPos = particleFallLastPos;
        particleFallPos.y += 12f;
        rainParticleT.position = particleFallPos;
        snowData.rootT.position = particleFallPos;
        for (int i = 0; i < 4; i++)
        {
            ParticleData particleData = stormData[i];
            particleData.rootT.position = particleFallPos;
            if ((bool)particleData.farT)
            {
                particleData.farT.position = particleFallPos;
            }
        }

        BiomeWeather biomeWeather = ((localPlayer.biomeStandingOn != null) ? FindBiomeWeather(localPlayer.biomeStandingOn.m_BiomeType) : currentWeather);
        float num = ((forceRain >= 0f) ? forceRain : biomeWeather.rainParam.value);
        if (num > 0f && (bool)(UnityEngine.Object)(object)rainParticleSys)
        {
            EmissionModule emission = rainParticleSys.emission;
            ((EmissionModule)(ref emission)).rateOverTime = MinMaxCurve.op_Implicit(rainEmissionMaxRate * (num * 0.99f + 0.01f));
        }

        rainParticleObj.SetActive(num > 0f);
        float num2 = ((forceSnowfall >= 0f) ? forceSnowfall : Utils.FastClamp01(biomeWeather.snowFallParam.value));
        if (num2 <= 0f)
        {
            snowData.rootObj.SetActive(value: false);
        }
        else
        {
            snowData.rootObj.SetActive(value: true);
            bool flag = SkyManager.GetFogDensity() < 0.3f;
            float num3 = num2 * 0.99f + 0.01f;
            EmissionModule emission2 = snowData.rootParticleSys.emission;
            ((EmissionModule)(ref emission2)).rateOverTime = MinMaxCurve.op_Implicit(snowData.rootEmissionMaxRate * num3);
            emission2 = snowData.nearParticleSys.emission;
            ((EmissionModule)(ref emission2)).rateOverTime = MinMaxCurve.op_Implicit(snowData.nearEmissionMaxRate * num3);
            emission2 = snowData.topParticleSys.emission;
            ((EmissionModule)(ref emission2)).rateOverTime = MinMaxCurve.op_Implicit(flag ? (snowData.topEmissionMaxRate * num3) : 0f);
            MainModule main = snowData.farParticleSys.main;
            MinMaxGradient startColor = ((MainModule)(ref main)).startColor;
            Color farBaseColor = snowData.farBaseColor;
            farBaseColor.a *= num2 * 0.95f + 0.05f;
            ((MinMaxGradient)(ref startColor)).color = farBaseColor;
            ((MainModule)(ref main)).startColor = startColor;
            Vector3 position2 = localPlayer.position - Origin.position;
            position2.y += 1f;
            snowData.forceT.position = position2;
        }

        for (int j = 0; j < 4; j++)
        {
            ParticleData particleData2 = stormData[j];
            BiomeWeather weather = particleData2.weather;
            int num4 = ((weather.biomeDefinition == biomeWeather.biomeDefinition) ? weather.biomeDefinition.currentWeatherGroup.stormLevel : 0);
            if (num4 == 0)
            {
                if (particleData2.intensity > -1f)
                {
                    particleData2.intensity -= 0.2f * deltaTime;
                    if (particleData2.intensity <= -1f)
                    {
                        particleData2.stormLevel = 0;
                        particleData2.thunderDelay = 0f;
                        if ((bool)particleData2.farT)
                        {
                            particleData2.farT.gameObject.SetActive(value: false);
                        }

                        particleData2.rootObj.SetActive(value: false);
                    }
                }
            }
            else
            {
                if (particleData2.stormLevel != num4)
                {
                    particleData2.stormLevel = num4;
                    particleData2.intensity = 0f;
                }

                particleData2.intensity += 0.2f * deltaTime;
                particleData2.intensity = Utils.FastClamp01(particleData2.intensity);
                particleData2.thunderDelay -= deltaTime;
                if (particleData2.thunderDelay <= 0f)
                {
                    particleData2.thunderDelay = gameRand.RandomRange(4, 12);
                    if (EnvironmentAudioManager.Instance != null)
                    {
                        EnvironmentAudioManager.Instance.TriggerThunder(localPlayer.position);
                    }
                }
            }

            if (!(particleData2.intensity > -1f))
            {
                continue;
            }

            float num5 = Utils.FastMax(0f, particleData2.intensity);
            if (num4 == 1)
            {
                if ((bool)particleData2.farT)
                {
                    particleData2.farT.gameObject.SetActive(value: true);
                    MainModule main2 = snowData.farParticleSys.main;
                    MinMaxGradient startColor2 = ((MainModule)(ref main2)).startColor;
                    Color farBaseColor2 = particleData2.farBaseColor;
                    farBaseColor2.a *= num5 * 0.95f + 0.05f;
                    ((MinMaxGradient)(ref startColor2)).color = farBaseColor2;
                    ((MainModule)(ref main2)).startColor = startColor2;
                    if ((bool)(UnityEngine.Object)(object)particleData2.ringParticleSys)
                    {
                        ShapeModule shape = particleData2.ringParticleSys.shape;
                        ((ShapeModule)(ref shape)).radius = Utils.FastLerp(0.3f, 1f, (float)(int)weather.remainingSeconds / 60f) * particleData2.ringRadiusMax;
                    }
                }

                particleData2.rootObj.SetActive(value: false);
            }
            else
            {
                if (num5 < 0.2f && (bool)(UnityEngine.Object)(object)particleData2.ringParticleSys)
                {
                    particleData2.ringParticleSys.Stop();
                }

                if (num5 > 0.99f && (bool)particleData2.farT)
                {
                    particleData2.farT.gameObject.SetActive(value: false);
                }

                particleData2.rootObj.SetActive(value: true);
                for (int k = 0; k < particleData2.particleSystems.Count; k++)
                {
                    EmissionModule emission3 = particleData2.particleSystems[k].emission;
                    ((EmissionModule)(ref emission3)).rateOverTime = MinMaxCurve.op_Implicit(particleData2.psEmissionMaxRates[k] * num5);
                }

                Vector3 position3 = localPlayer.position - Origin.position;
                position3.y += 2f;
                particleData2.forceT.position = position3;
            }
        }
    }

    public string GetSpectrumInfo()
    {
        float value = 1f - spectrumBlend;
        float value2 = spectrumBlend;
        string text = spectrumSourceType.ToString();
        string text2 = spectrumTargetType.ToString();
        return $"source {text} {value.ToCultureInvariantString()}, target {text2} {value2.ToCultureInvariantString()}";
    }

    public static void SetForceSpectrum(SpectrumWeatherType type)
    {
        forcedSpectrum = type;
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void SpectrumsFrameUpdate()
    {
        LoadSpectrums();
        float num = SkyManager.BloodMoonVisiblePercent();
        if (num > 0f && spectrumTargetType == SpectrumWeatherType.BloodMoon)
        {
            spectrumBlend = num;
        }
        else if (spectrumBlend < 1f)
        {
            spectrumBlend += Time.deltaTime / 10f;
            if (spectrumBlend > 1f)
            {
                spectrumBlend = 1f;
            }
        }

        if (spectrumSourceType == spectrumTargetType)
        {
            spectrumBlend = 1f;
        }

        if (spectrumBlend >= 1f)
        {
            spectrumSourceType = spectrumTargetType;
            spectrumTargetType = SpectrumWeatherType.Biome;
            if (currentWeather.biomeDefinition != null)
            {
                spectrumTargetType = currentWeather.biomeDefinition.weatherSpectrum;
            }

            if (num > 0f)
            {
                spectrumTargetType = SpectrumWeatherType.BloodMoon;
            }

            if (spectrumSourceType != spectrumTargetType)
            {
                spectrumBlend = 0f;
            }
        }
    }

    public Color GetWeatherSpectrum(Color regularSpectrum, AtmosphereEffect.ESpecIdx type, float dayTimeScalar)
    {
        if (forcedSpectrum != SpectrumWeatherType.None)
        {
            int num = (int)forcedSpectrum;
            AtmosphereEffect atmosphereEffect = atmosphereSpectrum[num];
            if (atmosphereEffect == null)
            {
                return regularSpectrum;
            }

            return atmosphereEffect.spectrums[(int)type]?.GetValue(dayTimeScalar) ?? regularSpectrum;
        }

        Color color = regularSpectrum;
        Color color2 = regularSpectrum;
        if (isGameModeNormal)
        {
            if (spectrumSourceType != 0)
            {
                ColorSpectrum colorSpectrum = atmosphereSpectrum[(int)spectrumSourceType].spectrums[(int)type];
                if (colorSpectrum != null)
                {
                    color = colorSpectrum.GetValue(dayTimeScalar);
                }
            }

            if (spectrumTargetType != 0)
            {
                ColorSpectrum colorSpectrum2 = atmosphereSpectrum[(int)spectrumTargetType].spectrums[(int)type];
                if (colorSpectrum2 != null)
                {
                    color2 = colorSpectrum2.GetValue(dayTimeScalar);
                }
            }
        }

        return color * (1f - spectrumBlend) + color2 * spectrumBlend;
    }

    public void TriggerThunder(int _playWorldTime, Vector3 _pos)
    {
        EnvironmentAudioManager.Instance.TriggerThunder(_pos);
    }

    public void ClientProcessPackages(WeatherPackage[] _packages)
    {
        int num = Time.frameCount;
        if (processingPackageFrame == num)
        {
            return;
        }

        processingPackageFrame = num;
        foreach (WeatherPackage weatherPackage in _packages)
        {
            if (WorldBiomes.Instance.TryGetBiome(weatherPackage.biomeId, out var _bd))
            {
                _bd.SetWeatherGroup(weatherPackage.groupIndex);
                BiomeWeather biomeWeather = FindBiomeWeather(weatherPackage.biomeId);
                if (biomeWeather != null)
                {
                    weatherPackage.CopyTo(biomeWeather);
                }
            }
        }
    }

    public void SendPackages()
    {
        CalcPackages();
        NetPackageWeather package = NetPackageManager.GetPackage<NetPackageWeather>();
        package.Setup(weatherPackages);
        SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: true);
    }

    [PublicizedFrom(EAccessModifier.Private)]
    public void CalcPackages()
    {
        int count = this.biomeWeather.Count;
        for (int i = 0; i < count; i++)
        {
            BiomeWeather biomeWeather = this.biomeWeather[i];
            WeatherPackage weatherPackage = weatherPackages[i];
            weatherPackage.biomeId = biomeWeather.biomeDefinition.m_Id;
            weatherPackage.groupIndex = (byte)biomeWeather.biomeDefinition.currentWeatherGroupIndex;
            weatherPackage.remainingSeconds = biomeWeather.remainingSeconds;
            for (int j = 0; j < biomeWeather.parameters.Length && j < weatherPackage.param.Length; j++)
            {
                weatherPackage.param[j] = biomeWeather.parameterFinals[j];
            }
        }
    }

    public override string ToString()
    {
        string text = $"#{this.biomeWeather.Count}";
        for (int i = 0; i < this.biomeWeather.Count; i++)
        {
            BiomeWeather biomeWeather = this.biomeWeather[i];
            text = text + "\n" + biomeWeather.ToString();
        }

        return text;
    }

    [Conditional("DEBUG_WEATHERNET")]
    public void LogNet(string _format = "", params object[] _args)
    {
        _format = $"{GameManager.frameCount} WeatherManager net {_format}";
        Log.Warning(_format, _args);
    }
}
