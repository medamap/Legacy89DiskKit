cmake_minimum_required(VERSION 3.20)

add_library(Legacy89DiskKitCpp STATIC
    src/character_encoding.cpp
    src/raw_disk_geometry.cpp
    src/d88_parser.cpp
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

enable_testing()
add_test(NAME Legacy89DiskKitCppSmoke COMMAND Legacy89DiskKitCppSmoke)
