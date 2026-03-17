cmake_minimum_required(VERSION 3.20)

option(LEGACY89DISKKIT_BUILD_SHARED "Build shared library for P/Invoke" OFF)

set(SOURCES
    src/character_encoding.cpp
    src/raw_disk_geometry.cpp
    src/d88_parser.cpp
    src/filesystem_surface_catalog.cpp
    src/infrastructure/disk_image/buffer_image_format.cpp
    src/infrastructure/disk_image/d88_buffer_loader.cpp
    src/infrastructure/disk_image/raw_buffer_loader.cpp
    src/infrastructure/disk_image/disk_image_buffer_loader.cpp
    src/infrastructure/disk_image/raw_disk_container.cpp
    src/infrastructure/disk_image/d88_disk_container.cpp
    src/infrastructure/drive/raw_sector_addressable_medium.cpp
    src/infrastructure/drive/d88_sector_addressable_medium.cpp
    src/infrastructure/drive/mounted_medium_binding_factory.cpp
    src/infrastructure/fdc/medium/sector_backed_controller_facing_medium.cpp
    src/infrastructure/fdc/medium/raw_backed_controller_facing_medium.cpp
    src/infrastructure/fdc/medium/d88_backed_controller_facing_medium.cpp
    src/infrastructure/character_encoding/byte_text_encoding_table.cpp
    src/infrastructure/character_encoding/x1_encoding_table.cpp
    src/infrastructure/character_encoding/character_encoding_table_catalog.cpp
    src/infrastructure/filesystem/filesystem_detection.cpp
    src/infrastructure/fdc/fdc_medium_controller.cpp
    src/infrastructure/native/native_file_system_session.cpp
    src/infrastructure/native/native_bridge_exports.cpp
    src/infrastructure/filesystem/explicit_filesystem_selector.cpp
    src/infrastructure/filesystem/hu_basic/hu_basic_file_system.cpp
    src/infrastructure/filesystem/n88_basic/n88_basic_file_system.cpp
    src/infrastructure/filesystem/msx_dos/explicit_msx_dos_file_system.cpp
    src/infrastructure/filesystem/msx_dos/msx_dos_file_system.cpp
    src/hu_basic_configuration.cpp
    src/hu_basic_name_rules.cpp
    src/hu_basic_fat_rules.cpp
    src/hu_basic_allocation_rules.cpp
    src/hu_basic_read_rules.cpp
    src/hu_basic_write_rules.cpp
    src/hu_basic_directory_rules.cpp
    src/hu_basic_directory_entry_codec.cpp
    src/hu_basic_default_attribute_rules.cpp
    src/hu_basic_delete_rules.cpp
    src/hu_basic_rename_rules.cpp
    src/hu_basic_attribute_update_rules.cpp
    src/hu_basic_directory_listing.cpp
    src/hu_basic_delete_transaction.cpp
    src/hu_basic_rename_transaction.cpp
    src/hu_basic_attribute_update_transaction.cpp
    src/hu_basic_directory_layout_parser.cpp
    src/hu_basic_file_lookup.cpp
    src/hu_basic_shell.cpp
    src/hu_basic_format_rules.cpp
    src/hu_basic_record_address_rules.cpp
    src/hu_basic_cluster_write_rules.cpp
    src/hu_basic_virtual_label_entry_rules.cpp
    src/hu_basic_mode_rules.cpp
    src/hu_basic_dir_parser.cpp
    src/hu_basic_boot_record_parser.cpp
    src/hu_basic_boot_record_codec.cpp
    src/hu_basic_file_entry_writer.cpp
    src/hu_basic_directory_layout_rules.cpp
    src/hu_basic_filesystem_info_rules.cpp
    src/hu_basic_label_rules.cpp
    src/hu_basic_directory_sector_rules.cpp
    src/hu_basic_write_transaction.cpp
    src/n88_basic_configuration.cpp
    src/n88_basic_dir_parser.cpp
    src/n88_basic_fat_rules.cpp
    src/n88_basic_read_rules.cpp
    src/n88_basic_directory_listing.cpp
    src/n88_basic_default_attribute_rules.cpp
    src/n88_basic_mode_rules.cpp
    src/n88_basic_write_rules.cpp
    src/n88_basic_allocation_rules.cpp
    src/n88_basic_delete_rules.cpp
    src/n88_basic_rename_rules.cpp
    src/n88_basic_attribute_update_rules.cpp
    src/n88_basic_format_rules.cpp
    src/n88_basic_file_lookup.cpp
    src/n88_basic_filesystem_info_rules.cpp
    src/n88_basic_file_entry_writer.cpp
    src/n88_basic_write_transaction.cpp
    src/n88_basic_delete_transaction.cpp
    src/n88_basic_rename_transaction.cpp
    src/n88_basic_attribute_update_transaction.cpp
    src/n88_basic_shell.cpp
    src/msx_dos_configuration.cpp
    src/msx_dos_boot_sector_parser.cpp
    src/msx_dos_mode_rules.cpp
    src/msx_dos_dir_parser.cpp
    src/msx_dos_fat_rules.cpp
    src/msx_dos_read_rules.cpp
    src/msx_dos_directory_listing.cpp
    src/msx_dos_default_attribute_rules.cpp
    src/msx_dos_file_lookup.cpp
    src/msx_dos_filesystem_info_rules.cpp
    src/msx_dos_file_entry_writer.cpp
    src/msx_dos_format_rules.cpp
    src/msx_dos_cluster_write_rules.cpp
    src/msx_dos_write_rules.cpp
    src/msx_dos_allocation_rules.cpp
    src/msx_dos_delete_rules.cpp
    src/msx_dos_rename_rules.cpp
    src/msx_dos_attribute_update_rules.cpp
    src/msx_dos_write_transaction.cpp
    src/msx_dos_delete_transaction.cpp
    src/msx_dos_rename_transaction.cpp
    src/msx_dos_attribute_update_transaction.cpp
    src/msx_dos_shell.cpp
    src/application/disk_service.cpp
    src/application/character_encoding_service.cpp
    src/application/file_transfer_service.cpp
    src/application/directory_layout_service.cpp
    src/application/boot_and_clone_service.cpp
    src/application/explicit_file_system_resolver.cpp
    src/application/legacy89diskkit_application.cpp
    src/application/drive_mount_service.cpp
    src/application/mounted_medium_binding_service.cpp
    src/application/fdc_access_service.cpp
    src/application/event_driven_emulator_fdc_host_adapter.cpp
)

add_library(Legacy89DiskKitCpp STATIC ${SOURCES})

target_include_directories(Legacy89DiskKitCpp
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/include
        ${CMAKE_CURRENT_SOURCE_DIR}/../../include
)

target_compile_features(Legacy89DiskKitCpp PUBLIC cxx_std_20)

if(LEGACY89DISKKIT_BUILD_SHARED)
    add_library(Legacy89DiskKitCppShared SHARED ${SOURCES})
    target_include_directories(Legacy89DiskKitCppShared
        PUBLIC
            ${CMAKE_CURRENT_SOURCE_DIR}/include
            ${CMAKE_CURRENT_SOURCE_DIR}/../../include
    )
    target_compile_features(Legacy89DiskKitCppShared PUBLIC cxx_std_20)
    target_compile_definitions(Legacy89DiskKitCppShared PRIVATE LEGACY89DISKKIT_EXPORTS)
    set_target_properties(Legacy89DiskKitCppShared PROPERTIES
        OUTPUT_NAME "Legacy89DiskKitCpp"
    )
endif()

add_executable(Legacy89DiskKitCppSmoke
    tests/raw_and_d88_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppHuBasicSmoke
    tests/hu_basic_rules_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppHuBasicSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppMsxDosSmoke
    tests/msx_dos_rules_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppMsxDosSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppSurfaceCatalogSmoke
    tests/filesystem_surface_catalog_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppSurfaceCatalogSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppControllerDomainSmoke
    tests/controller_domain_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppControllerDomainSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppRawPreservationDomainSmoke
    tests/raw_preservation_domain_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppRawPreservationDomainSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppBufferLoaderSmoke
    tests/disk_image_buffer_loader_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppBufferLoaderSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppRawDiskContainerSmoke
    tests/raw_disk_container_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppRawDiskContainerSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppD88DiskContainerSmoke
    tests/d88_disk_container_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppD88DiskContainerSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppCharacterEncodingTableSmoke
    tests/character_encoding_table_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppCharacterEncodingTableSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppHuBasicFileSystemSmoke
    tests/hu_basic_file_system_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppHuBasicFileSystemSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppN88BasicFileSystemSmoke
    tests/n88_basic_file_system_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppN88BasicFileSystemSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppMsxDosFileSystemSmoke
    tests/msx_dos_file_system_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppMsxDosFileSystemSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppExplicitFileSystemSelectorSmoke
    tests/explicit_filesystem_selector_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppExplicitFileSystemSelectorSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppFileSystemDetectionSmoke
    tests/filesystem_detection_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppFileSystemDetectionSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppMountedMediumInfrastructureSmoke
    tests/mounted_medium_infrastructure_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppMountedMediumInfrastructureSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppNativeFileSystemSessionSmoke
    tests/native_file_system_session_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppNativeFileSystemSessionSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppNativeBridgeExportsSmoke
    tests/native_bridge_exports_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppNativeBridgeExportsSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppDiskServiceSmoke
    tests/disk_service_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppDiskServiceSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppFileTransferServiceSmoke
    tests/file_transfer_service_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppFileTransferServiceSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppDirectoryLayoutServiceSmoke
    tests/directory_layout_service_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppDirectoryLayoutServiceSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppBootAndCloneServiceSmoke
    tests/boot_and_clone_service_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppBootAndCloneServiceSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppBootstrapSmoke
    tests/bootstrap_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppBootstrapSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppFdcApplicationSmoke
    tests/fdc_application_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppFdcApplicationSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(ldk-verify
    src/presentation/ldk_verify.cpp
)

target_link_libraries(ldk-verify PRIVATE Legacy89DiskKitCpp)

enable_testing()
add_test(NAME Legacy89DiskKitCppSmoke COMMAND Legacy89DiskKitCppSmoke)
add_test(NAME Legacy89DiskKitCppHuBasicSmoke COMMAND Legacy89DiskKitCppHuBasicSmoke)
add_test(NAME Legacy89DiskKitCppMsxDosSmoke COMMAND Legacy89DiskKitCppMsxDosSmoke)
add_test(NAME Legacy89DiskKitCppSurfaceCatalogSmoke COMMAND Legacy89DiskKitCppSurfaceCatalogSmoke)
add_test(NAME Legacy89DiskKitCppControllerDomainSmoke COMMAND Legacy89DiskKitCppControllerDomainSmoke)
add_test(NAME Legacy89DiskKitCppRawPreservationDomainSmoke COMMAND Legacy89DiskKitCppRawPreservationDomainSmoke)
add_test(NAME Legacy89DiskKitCppBufferLoaderSmoke COMMAND Legacy89DiskKitCppBufferLoaderSmoke)
add_test(NAME Legacy89DiskKitCppRawDiskContainerSmoke COMMAND Legacy89DiskKitCppRawDiskContainerSmoke)
add_test(NAME Legacy89DiskKitCppD88DiskContainerSmoke COMMAND Legacy89DiskKitCppD88DiskContainerSmoke)
add_test(NAME Legacy89DiskKitCppCharacterEncodingTableSmoke COMMAND Legacy89DiskKitCppCharacterEncodingTableSmoke)
add_test(NAME Legacy89DiskKitCppHuBasicFileSystemSmoke COMMAND Legacy89DiskKitCppHuBasicFileSystemSmoke)
add_test(NAME Legacy89DiskKitCppN88BasicFileSystemSmoke COMMAND Legacy89DiskKitCppN88BasicFileSystemSmoke)
add_test(NAME Legacy89DiskKitCppMsxDosFileSystemSmoke COMMAND Legacy89DiskKitCppMsxDosFileSystemSmoke)
add_test(NAME Legacy89DiskKitCppExplicitFileSystemSelectorSmoke COMMAND Legacy89DiskKitCppExplicitFileSystemSelectorSmoke)
add_test(NAME Legacy89DiskKitCppFileSystemDetectionSmoke COMMAND Legacy89DiskKitCppFileSystemDetectionSmoke)
add_test(NAME Legacy89DiskKitCppMountedMediumInfrastructureSmoke COMMAND Legacy89DiskKitCppMountedMediumInfrastructureSmoke)
add_test(NAME Legacy89DiskKitCppNativeFileSystemSessionSmoke COMMAND Legacy89DiskKitCppNativeFileSystemSessionSmoke)
add_test(NAME Legacy89DiskKitCppNativeBridgeExportsSmoke COMMAND Legacy89DiskKitCppNativeBridgeExportsSmoke)
add_test(NAME Legacy89DiskKitCppDiskServiceSmoke COMMAND Legacy89DiskKitCppDiskServiceSmoke)
add_test(NAME Legacy89DiskKitCppFileTransferServiceSmoke COMMAND Legacy89DiskKitCppFileTransferServiceSmoke)
add_test(NAME Legacy89DiskKitCppDirectoryLayoutServiceSmoke COMMAND Legacy89DiskKitCppDirectoryLayoutServiceSmoke)
add_test(NAME Legacy89DiskKitCppBootAndCloneServiceSmoke COMMAND Legacy89DiskKitCppBootAndCloneServiceSmoke)
add_test(NAME Legacy89DiskKitCppBootstrapSmoke COMMAND Legacy89DiskKitCppBootstrapSmoke)
add_test(NAME Legacy89DiskKitCppFdcApplicationSmoke COMMAND Legacy89DiskKitCppFdcApplicationSmoke)
