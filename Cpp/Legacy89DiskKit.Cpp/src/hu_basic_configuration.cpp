#include "legacy89diskkit/cpp/hu_basic_configuration.hpp"

#include <stdexcept>

namespace legacy89diskkit::cpp
{
HuBasicConfiguration HuBasicConfigurationProvider::GetDefault(DiskType disk_type)
{
    switch (disk_type)
    {
    case DiskType::TwoD:
        return HuBasicConfiguration{ 2, 80, 16 * 256, 256 };
    case DiskType::TwoDD:
        return HuBasicConfiguration{ 2, 160, 16 * 256, 256 };
    case DiskType::TwoHD:
        return HuBasicConfiguration{ 3, 250, 16 * 256, 256 };
    default:
        throw std::invalid_argument("Unsupported disk type for Hu-BASIC");
    }
}
}
