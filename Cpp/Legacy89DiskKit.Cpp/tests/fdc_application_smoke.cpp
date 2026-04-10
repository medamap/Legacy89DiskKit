#include "legacy89diskkit/cpp/application/drive_mount_service.hpp"
#include "legacy89diskkit/cpp/application/mounted_medium_binding_service.hpp"
#include "legacy89diskkit/cpp/application/fdc_access_service.hpp"
#include "legacy89diskkit/cpp/application/event_driven_emulator_fdc_host_adapter.hpp"
#include "legacy89diskkit/cpp/infrastructure/native/native_file_system_session.hpp"

#include <iostream>
#include <vector>
#include <filesystem>
#include <cassert>

using namespace legacy89diskkit::cpp;
using namespace legacy89diskkit::cpp::application;

namespace
{
struct TempFile
{
    std::filesystem::path path;
    explicit TempFile(std::filesystem::path p) : path(std::move(p)) 
    {
        if (std::filesystem::exists(path)) std::filesystem::remove(path);
    }
    ~TempFile()
    {
        if (std::filesystem::exists(path)) std::filesystem::remove(path);
    }
    std::string string() const { return path.string(); }
};

std::filesystem::path GetTempPath(const std::string& filename)
{
    return std::filesystem::temp_directory_path() / filename;
}
}

int main()
{
    TempFile disk_file(GetTempPath("fdc_app_smoke.d88"));

    // 1. Setup Infrastructure and Application Services
    auto drive_mount_service = std::make_shared<DriveMountService>();
    auto binding_service = std::make_shared<MountedMediumBindingService>();
    
    // 2. Setup Disk and Session
    auto create_result = NativeFileSystemSession::Create(disk_file.string(), DiskType::TwoD, "FDC_TEST");
    assert(create_result.ok());
    auto session = std::move(create_result.value());

    // 3. Setup Adapter
    EventDrivenEmulatorFdcHostAdapter adapter(drive_mount_service, binding_service);
    
    bool irq_fired = false;
    adapter.on_irq_changed = [&](bool state) {
        irq_fired = state;
        std::cout << "IRQ State Changed: " << (state ? "ON" : "OFF") << std::endl;
    };

    // 4. Open Disk via Adapter
    std::cout << "Opening Disk via Adapter..." << std::endl;
    adapter.OpenDisk(0, session);
    assert(adapter.IsDiskInserted(0));
    
    // Test is_ready logic (requires medium + motor_on)
    std::cout << "Testing is_ready logic..." << std::endl;
    assert(!adapter.IsDriveReady(0)); // Motor is OFF initially
    
    adapter.SetMotorOn(0, true);
    assert(adapter.IsDriveReady(0)); // Motor is now ON
    
    adapter.SetMotorOn(0, false);
    assert(!adapter.IsDriveReady(0)); // Motor is now OFF again

    // 5. Test FDC Access via adapter methods
    std::cout << "Testing FDC Registers and IRQ..." << std::endl;
    adapter.SelectDrive(0);
    adapter.WriteIo8(1, 10); // Track register
    assert(adapter.ReadIo8(1) == 10);
    
    // In this smoke test environment, we verify that callbacks are connected.
    // SyncSignals() is called inside OpenDisk, SelectDrive, etc.
    // Since the mock session doesn't fire real IRQs yet, we just verify the adapter state is consistent.
    auto visible = adapter.Handle({EmulatorHostRequestKind::QueryState}).visible_state;
    assert(visible.has_value());
    assert(visible->selected_drive == 0);

    // 6. Test Protocol Handler (Handle method)
    std::cout << "Testing Protocol Handler..." << std::endl;
    EmulatorHostRequest req;
    req.kind = EmulatorHostRequestKind::ReadRegister;
    req.register_address = 1;
    
    auto resp = adapter.Handle(req);
    assert(!resp.error_message.has_value());
    assert(resp.register_value.has_value());
    assert(resp.register_value.value() == 10);

    // Test Error Handling in protocol
    std::cout << "Testing Protocol Error Reporting..." << std::endl;
    EmulatorHostRequest bad_req;
    bad_req.kind = EmulatorHostRequestKind::OpenDiskPath; // Not implemented yet
    auto err_resp = adapter.Handle(bad_req);
    assert(err_resp.error_message.has_value());
    std::cout << "Captured expected error: " << *err_resp.error_message << std::endl;

    // 7. Test Reset
    adapter.Reset();
    assert(adapter.ReadIo8(1) == 0); // Should be reset to 0

    std::cout << "FdcApplication smoke tests passed!" << std::endl;
    return 0;
}
