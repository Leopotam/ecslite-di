# LeoECS Lite DI - Поддержка автоматической инъекции данных в поля ECS-систем
Обеспечивает поддержку инъекции пользовательских и ECS-данных в поля ECS-систем для LeoECS Lite.

> Проверено на Unity 2020.3 (не зависит от Unity) и содержит asmdef-описания для компиляции в виде отдельных сборок и уменьшения времени рекомпиляции основного проекта.

# Содержание
* [Социальные ресурсы](#Социальные-ресурсы)
* [Установка](#Установка)
    * [В виде unity модуля](#В-виде-unity-модуля)
    * [В виде исходников](#В-виде-исходников)
* [Интеграция](#Интеграция)
* [Классы](#Классы)
    * [EcsWorldInject](#EcsWorldInject)
    * [EcsPoolInject](#EcsPoolInject)
    * [EcsFilterInject](#EcsFilterInject)
    * [EcsSharedInject](#EcsSharedInject)
    * [EcsCustomInject](#EcsCustomInject)
* [Лицензия](#Лицензия)

# Социальные ресурсы
[![discord](https://img.shields.io/discord/404358247621853185.svg?label=enter%20to%20discord%20server&style=for-the-badge&logo=discord)](https://discord.gg/5GZVde6)

# Установка

## В виде unity модуля
Поддерживается установка в виде unity-модуля через git-ссылку в PackageManager или прямое редактирование `Packages/manifest.json`:
```
"com.leopotam.ecslite.di": "https://github.com/Leopotam/ecslite-di.git",
```
По умолчанию используется последняя релизная версия. Если требуется версия "в разработке" с актуальными изменениями - следует переключиться на ветку `develop`:
```
"com.leopotam.ecslite.di": "https://github.com/Leopotam/ecslite-di.git#develop",
```

## В виде исходников
Код так же может быть склонирован или получен в виде архива со страницы релизов.

# Интеграция
```c#
var systems = new EcsSystems (new EcsWorld ());
systems
    .Add (new System1 ())
    .AddWorld (new EcsWorld (), "events")
    // ...
    // Вызов Inject() должен быть размещен после регистрации
    // всех систем и миров, но до вызова Init().
    .Inject ()
    .Init ();
```

# Классы

## EcsWorldInject
```c#
class TestSystem : IEcsRunSystem {
    // Поле будет содержать ссылку на мир "по умолчанию".
    readonly EcsWorldInject _defaultWorld = default;
    
    // Поле будет содержать ссылку на мир "events".
    readonly EcsWorldInject _eventsWorld = "events";

    public void Run (EcsSystems systems) {
        // Все поля заполнены и могут быть использованы:
        // _defaultWorld.Value.xxx
        // _eventsWorld.Value.xxx
    }
}
```

## EcsPoolInject
```c#
class TestSystem : IEcsRunSystem {
    // Поле будет содержать ссылку на пул из мира "по умолчанию".
    readonly EcsPoolInject<C1> _c1Pool = default;
    
    // Поле будет содержать ссылку на пул из мира "events".
    readonly EcsPoolInject<C1> _c1EventsPool = "events";

    public void Run (EcsSystems systems) {
        // Все поля заполнены и могут быть использованы:
        // _c1Pool.Value.xxx
        // _c1EventsPool.Value.xxx
    }
}
```

## EcsFilterInject
```c#
class TestSystem : IEcsRunSystem {
    // Поле будет содержать ссылку на фильтр (с C1) из мира "по умолчанию".
    readonly EcsFilterInject<Inc<C1>> _filter1 = default;
    
    // Поле будет содержать ссылку на фильтр (с C1 и C2) из мира "по умолчанию".
    readonly EcsFilterInject<Inc<C1, C2>> _filter2 = default;
    
    // Поле будет содержать ссылку на фильтр (с C1, но без C2) из мира "по умолчанию".
    readonly EcsFilter<Inc<C1>, Exc<C2>> _filter11 = default;
    
    // Поле будет содержать ссылку на фильтр (с C1, но без C2) из мира "events".
    readonly EcsFilter<Inc<C1>, Exc<C2>> _eventsFilter11 = "events";

    public void Run (EcsSystems systems) {
        // Все поля заполнены и могут быть использованы:
        // _filter1.Value.xxx
        // _filter2.Value.xxx
        // _filter11.Value.xxx
        // _eventsFilter11.Value.xxx
        // В том числе и пулы, использовавшиеся в определении фильтров:
        // EcsPool<C1> pool1 = _filter2.Pools.Inc1;
        // EcsPool<C2> pool2 = _filter2.Pools.Inc2;
    }
}
```
**ВАЖНО!** `Inc<>` определен для компонентов количеством до 8, `Exc<>` определен для компонентов количеством до 4.
Если есть необходимость использовать больше - это можно сделать через определение нового класса ограничения внутри своем проекте.
Например, `Inc<>` с поддержкой 10 компонентов:
```c#
public struct Inc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IEcsInclude
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
    where T5 : struct
    where T6 : struct
    where T7 : struct
    where T8 : struct
    where T9 : struct
    where T10 : struct {
    public EcsPool<T1> Inc1;
    public EcsPool<T2> Inc2;
    public EcsPool<T3> Inc3;
    public EcsPool<T4> Inc4;
    public EcsPool<T5> Inc5;
    public EcsPool<T6> Inc6;
    public EcsPool<T7> Inc7;
    public EcsPool<T8> Inc8;
    public EcsPool<T9> Inc9;
    public EcsPool<T10> Inc10;
        
    public EcsWorld.Mask Fill (EcsWorld world) {
        Inc1 = world.GetPool<T1> ();
        Inc2 = world.GetPool<T2> ();
        Inc3 = world.GetPool<T3> ();
        Inc4 = world.GetPool<T4> ();
        Inc5 = world.GetPool<T5> ();
        Inc6 = world.GetPool<T6> ();
        Inc7 = world.GetPool<T7> ();
        Inc8 = world.GetPool<T8> ();
        Inc9 = world.GetPool<T9> ();
        Inc10 = world.GetPool<T10> ();
        return world
            .Filter<T1> ()
            .Inc<T2> ()
            .Inc<T3> ()
            .Inc<T4> ()
            .Inc<T5> ()
            .Inc<T6> ()
            .Inc<T7> ()
            .Inc<T8> ()
            .Inc<T9> ()
            .Inc<T10> ();
    }
}
```
Аналогично, `Exc<>` для 6 компонентов:
```c#
public struct Exc<T1, T2, T3, T4, T5, T6> : IEcsExclude
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
    where T5 : struct
    where T6 : struct {
    public EcsWorld.Mask Fill (EcsWorld.Mask mask) {
        return mask.Exc<T1> ()
            .Exc<T2> ()
            .Exc<T3> ()
            .Exc<T4> ()
            .Exc<T5> ()
            .Exc<T6> ();
    }
}
```

## EcsSharedInject
```c#
class TestSystem : IEcsRunSystem {
    // Поле будет содержать ссылку на GetShared() объект.
    readonly EcsSharedInject<Shared> _shared = default;

    public void Run (EcsSystems systems) {
        // Все поля заполнены и могут быть использованы:
        // _shared.Value.xxx
    }
}
```

## EcsCustomInject
```c#
systems
    .Add (new TestSystem ())
    .Inject (new CustomData1 (), new CustomData2 ())
    .Init ();
// ...
class TestSystem : IEcsRunSystem {
    // Поле будет содержать ссылку на объект совместимого типа, переданого в вызов EcsSystems.Inject(xxx).
    readonly EcsCustomInject<CustomData1> _custom1 = default;
    
    // Поле будет содержать ссылку на объект совместимого типа, переданого в вызов EcsSystems.Inject(xxx).
    readonly EcsCustomInject<CustomData2> _custom2 = default;

    public void Run (EcsSystems systems) {
        // Все поля заполнены и могут быть использованы:
        // _custom1.Value.xxx
        // _custom2.Value.xxx
    }
}
```

# Лицензия
Фреймворк выпускается под двумя лицензиями, [подробности тут](./LICENSE.md).

В случаях лицензирования по условиям MIT-Red не стоит расчитывать на
персональные консультации или какие-либо гарантии.