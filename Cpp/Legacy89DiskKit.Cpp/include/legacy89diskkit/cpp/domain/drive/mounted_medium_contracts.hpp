#pragma once

#include <string>

namespace legacy89diskkit::cpp
{
class MountedMedium
{
public:
    virtual ~MountedMedium() = default;

    virtual const std::string& MediumKind() const = 0;
    virtual bool SupportsDirectImageAccess() const = 0;
    virtual bool SupportsControllerFacingAccess() const = 0;
};
}
