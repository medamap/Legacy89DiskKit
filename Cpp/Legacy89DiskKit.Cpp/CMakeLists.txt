cmake_minimum_required(VERSION 3.20)

add_library(Legacy89DiskKitCpp STATIC
    src/character_encoding.cpp
    src/raw_disk_geometry.cpp
    src/d88_parser.cpp
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
    src/n88_basic_shell.cpp
)

target_include_directories(Legacy89DiskKitCpp
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_compile_features(Legacy89DiskKitCpp PUBLIC cxx_std_20)

add_executable(Legacy89DiskKitCppSmoke
    tests/raw_and_d88_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppSmoke PRIVATE Legacy89DiskKitCpp)

add_executable(Legacy89DiskKitCppHuBasicSmoke
    tests/hu_basic_rules_smoke.cpp
)

target_link_libraries(Legacy89DiskKitCppHuBasicSmoke PRIVATE Legacy89DiskKitCpp)

enable_testing()
add_test(NAME Legacy89DiskKitCppSmoke COMMAND Legacy89DiskKitCppSmoke)
add_test(NAME Legacy89DiskKitCppHuBasicSmoke COMMAND Legacy89DiskKitCppHuBasicSmoke)
