#include "legacy89diskkit/cpp/n88_basic_configuration.hpp"

#include <stdexcept>

namespace legacy89diskkit::cpp
{
N88BasicConfiguration N88BasicConfigurationProvider::GetDefault(const DiskType disk_type)
{
    switch (disk_type)
    {
    case DiskType::TwoD:
        return N88BasicConfiguration{ 18, 1, 1, 12, 14, 3, 13, 256, 2048, 160, 0, 16 };
    case DiskType::TwoDD:
        return N88BasicConfiguration{ 40, 0, 1, 12, 14, 3, 13, 256, 4096, 160, 0, 16 };
    default:
        throw std::invalid_argument("Unsupported disk type for N88-BASIC");
    }
}
}
