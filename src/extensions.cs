// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Reflection;

namespace Leopotam.EcsLite.Di {
    public static class Extensions {
        public static EcsSystems Inject (this EcsSystems systems, params object[] injects) {
            if (injects == null) { injects = Array.Empty<object> (); }
            IEcsSystem[] allSystems = null;
            var systemsCount = systems.GetAllSystems (ref allSystems);

            for (var i = 0; i < systemsCount; i++) {
                var system = allSystems[i];
                foreach (var f in system.GetType ().GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                    // skip statics.
                    if (f.IsStatic) { continue; }
                    // EcsWorldInject, EcsFilterInject, EcsPoolInject, EcsSharedInject.
                    if (InjectBuiltIns (f, system, systems)) { continue; }
                    // EcsDataInject.
                    if (InjectCustoms (f, system, injects)) { continue; }
                }
            }

            return systems;
        }

        static bool InjectBuiltIns (FieldInfo fieldInfo, IEcsSystem system, EcsSystems systems) {
            if (typeof (IEcsDataInject).IsAssignableFrom (fieldInfo.FieldType)) {
                var instance = (IEcsDataInject) fieldInfo.GetValue (system);
                instance.Fill (systems);
                fieldInfo.SetValue (system, instance);
                return true;
            }
            return false;
        }

        static bool InjectCustoms (FieldInfo fieldInfo, IEcsSystem system, object[] injects) {
            if (typeof (IEcsCustomDataInject).IsAssignableFrom (fieldInfo.FieldType)) {
                var instance = (IEcsCustomDataInject) fieldInfo.GetValue (system);
                instance.Fill (injects);
                fieldInfo.SetValue (system, instance);
                return true;
            }
            return false;
        }
    }

    public interface IEcsDataInject {
        void Fill (EcsSystems systems);
    }

    public interface IEcsCustomDataInject {
        void Fill (object[] injects);
    }

    public interface IEcsInclude {
        EcsWorld.Mask Fill (EcsWorld world);
    }

    public interface IEcsExclude {
        EcsWorld.Mask Fill (EcsWorld.Mask mask);
    }

    public struct EcsFilterInject<TInc> : IEcsDataInject
        where TInc : struct, IEcsInclude {
        public EcsFilter Value;
        public TInc Pools;
        string _worldName;

        public static implicit operator EcsFilterInject<TInc> (string worldName) {
            return new EcsFilterInject<TInc> { _worldName = worldName };
        }

        void IEcsDataInject.Fill (EcsSystems systems) {
            Pools = default;
            Value = Pools.Fill (systems.GetWorld (_worldName)).End ();
        }
    }

    public struct EcsFilterInject<TInc, TExc> : IEcsDataInject
        where TInc : struct, IEcsInclude
        where TExc : struct, IEcsExclude {
        public EcsFilter Value;
        public TInc Pools;
        string _worldName;

        public static implicit operator EcsFilterInject<TInc, TExc> (string worldName) {
            return new EcsFilterInject<TInc, TExc> { _worldName = worldName };
        }

        void IEcsDataInject.Fill (EcsSystems systems) {
            Pools = default;
            TExc exc = default;
            Value = exc.Fill (Pools.Fill (systems.GetWorld (_worldName))).End ();
        }
    }

    public struct EcsPoolInject<T> : IEcsDataInject where T : struct {
        public EcsPool<T> Value;
        string _worldName;

        public static implicit operator EcsPoolInject<T> (string worldName) {
            return new EcsPoolInject<T> { _worldName = worldName };
        }

        void IEcsDataInject.Fill (EcsSystems systems) {
            Value = systems.GetWorld (_worldName).GetPool<T> ();
        }
    }

    public struct EcsSharedInject<T> : IEcsDataInject where T : class {
        public T Value;

        void IEcsDataInject.Fill (EcsSystems systems) {
            Value = systems.GetShared<T> ();
        }
    }

    public struct EcsCustomInject<T> : IEcsCustomDataInject where T : class {
        public T Value;

        void IEcsCustomDataInject.Fill (object[] injects) {
            if (injects.Length > 0) {
                var vType = typeof (T);
                foreach (var inject in injects) {
                    if (vType.IsInstanceOfType (inject)) {
                        Value = (T) inject;
                        break;
                    }
                }
            }
        }
    }

    public struct EcsWorldInject : IEcsDataInject {
        public EcsWorld Value;
        string _worldName;

        public static implicit operator EcsWorldInject (string worldName) {
            return new EcsWorldInject { _worldName = worldName };
        }

        void IEcsDataInject.Fill (EcsSystems systems) {
            Value = systems.GetWorld (_worldName);
        }
    }

    public struct Inc<T1> : IEcsInclude
        where T1 : struct {
        public EcsPool<T1> Inc1;

        public EcsWorld.Mask Fill (EcsWorld world) {
            Inc1 = world.GetPool<T1> ();
            return world.Filter<T1> ();
        }

        public EcsWorld.Mask Exclude (EcsWorld.Mask mask) {
            return mask.Exc<T1> ();
        }
    }

    public struct Inc<T1, T2> : IEcsInclude
        where T1 : struct where T2 : struct {
        public EcsPool<T1> Inc1;
        public EcsPool<T2> Inc2;

        public EcsWorld.Mask Fill (EcsWorld world) {
            Inc1 = world.GetPool<T1> ();
            Inc2 = world.GetPool<T2> ();
            return world.Filter<T1> ().Inc<T2> ();
        }
    }

    public struct Inc<T1, T2, T3> : IEcsInclude
        where T1 : struct where T2 : struct where T3 : struct {
        public EcsPool<T1> Inc1;
        public EcsPool<T2> Inc2;
        public EcsPool<T3> Inc3;

        public EcsWorld.Mask Fill (EcsWorld world) {
            Inc1 = world.GetPool<T1> ();
            Inc2 = world.GetPool<T2> ();
            Inc3 = world.GetPool<T3> ();
            return world.Filter<T1> ().Inc<T2> ().Inc<T3> ();
        }
    }

    public struct Inc<T1, T2, T3, T4> : IEcsInclude
        where T1 : struct where T2 : struct where T3 : struct where T4 : struct {
        public EcsPool<T1> Inc1;
        public EcsPool<T2> Inc2;
        public EcsPool<T3> Inc3;
        public EcsPool<T4> Inc4;

        public EcsWorld.Mask Fill (EcsWorld world) {
            Inc1 = world.GetPool<T1> ();
            Inc2 = world.GetPool<T2> ();
            Inc3 = world.GetPool<T3> ();
            Inc4 = world.GetPool<T4> ();
            return world.Filter<T1> ().Inc<T2> ().Inc<T3> ().Inc<T4> ();
        }
    }

    public struct Inc<T1, T2, T3, T4, T5> : IEcsInclude
        where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct {
        public EcsPool<T1> Inc1;
        public EcsPool<T2> Inc2;
        public EcsPool<T3> Inc3;
        public EcsPool<T4> Inc4;
        public EcsPool<T5> Inc5;

        public EcsWorld.Mask Fill (EcsWorld world) {
            Inc1 = world.GetPool<T1> ();
            Inc2 = world.GetPool<T2> ();
            Inc3 = world.GetPool<T3> ();
            Inc4 = world.GetPool<T4> ();
            Inc5 = world.GetPool<T5> ();
            return world.Filter<T1> ().Inc<T2> ().Inc<T3> ().Inc<T4> ().Inc<T5> ();
        }
    }

    public struct Inc<T1, T2, T3, T4, T5, T6> : IEcsInclude
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        where T6 : struct {
        public EcsPool<T1> Inc1;
        public EcsPool<T2> Inc2;
        public EcsPool<T3> Inc3;
        public EcsPool<T4> Inc4;
        public EcsPool<T5> Inc5;
        public EcsPool<T6> Inc6;

        public EcsWorld.Mask Fill (EcsWorld world) {
            Inc1 = world.GetPool<T1> ();
            Inc2 = world.GetPool<T2> ();
            Inc3 = world.GetPool<T3> ();
            Inc4 = world.GetPool<T4> ();
            Inc5 = world.GetPool<T5> ();
            Inc6 = world.GetPool<T6> ();
            return world.Filter<T1> ().Inc<T2> ().Inc<T3> ().Inc<T4> ().Inc<T5> ().Inc<T6> ();
        }
    }

    public struct Inc<T1, T2, T3, T4, T5, T6, T7> : IEcsInclude
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        where T6 : struct
        where T7 : struct {
        public EcsPool<T1> Inc1;
        public EcsPool<T2> Inc2;
        public EcsPool<T3> Inc3;
        public EcsPool<T4> Inc4;
        public EcsPool<T5> Inc5;
        public EcsPool<T6> Inc6;
        public EcsPool<T7> Inc7;

        public EcsWorld.Mask Fill (EcsWorld world) {
            Inc1 = world.GetPool<T1> ();
            Inc2 = world.GetPool<T2> ();
            Inc3 = world.GetPool<T3> ();
            Inc4 = world.GetPool<T4> ();
            Inc5 = world.GetPool<T5> ();
            Inc6 = world.GetPool<T6> ();
            Inc7 = world.GetPool<T7> ();
            return world.Filter<T1> ().Inc<T2> ().Inc<T3> ().Inc<T4> ().Inc<T5> ().Inc<T6> ().Inc<T7> ();
        }
    }

    public struct Inc<T1, T2, T3, T4, T5, T6, T7, T8> : IEcsInclude
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        where T6 : struct
        where T7 : struct
        where T8 : struct {
        public EcsPool<T1> Inc1;
        public EcsPool<T2> Inc2;
        public EcsPool<T3> Inc3;
        public EcsPool<T4> Inc4;
        public EcsPool<T5> Inc5;
        public EcsPool<T6> Inc6;
        public EcsPool<T7> Inc7;
        public EcsPool<T8> Inc8;

        public EcsWorld.Mask Fill (EcsWorld world) {
            Inc1 = world.GetPool<T1> ();
            Inc2 = world.GetPool<T2> ();
            Inc3 = world.GetPool<T3> ();
            Inc4 = world.GetPool<T4> ();
            Inc5 = world.GetPool<T5> ();
            Inc6 = world.GetPool<T6> ();
            Inc7 = world.GetPool<T7> ();
            Inc8 = world.GetPool<T8> ();
            return world.Filter<T1> ().Inc<T2> ().Inc<T3> ().Inc<T4> ().Inc<T5> ().Inc<T6> ().Inc<T7> ().Inc<T8> ();
        }
    }

    public struct Exc<T1> : IEcsExclude
        where T1 : struct {
        public EcsWorld.Mask Fill (EcsWorld.Mask mask) {
            return mask.Exc<T1> ();
        }
    }

    public struct Exc<T1, T2> : IEcsExclude
        where T1 : struct where T2 : struct {
        public EcsWorld.Mask Fill (EcsWorld.Mask mask) {
            return mask.Exc<T1> ().Exc<T2> ();
        }
    }

    public struct Exc<T1, T2, T3> : IEcsExclude
        where T1 : struct where T2 : struct where T3 : struct {
        public EcsWorld.Mask Fill (EcsWorld.Mask mask) {
            return mask.Exc<T1> ().Exc<T2> ().Exc<T3> ();
        }
    }

    public struct Exc<T1, T2, T3, T4> : IEcsExclude
        where T1 : struct where T2 : struct where T3 : struct where T4 : struct {
        public EcsWorld.Mask Fill (EcsWorld.Mask mask) {
            return mask.Exc<T1> ().Exc<T2> ().Exc<T3> ().Exc<T4> ();
        }
    }
}