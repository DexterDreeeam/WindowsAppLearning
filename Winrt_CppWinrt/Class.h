#pragma once

#include "Class.g.h"

namespace winrt::Winrt_CppWinrt::implementation
{
    struct Class : ClassT<Class>
    {
        Class() = default;

        int32_t MyProperty();
        void MyProperty(int32_t value);
    };
}

namespace winrt::Winrt_CppWinrt::factory_implementation
{
    struct Class : ClassT<Class, implementation::Class>
    {
    };
}
