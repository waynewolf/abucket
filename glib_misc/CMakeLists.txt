cmake_minimum_required(VERSION 3.0.2)
project(glib_misc C)

set(CMAKE_C_STANDARD 99)

add_executable(glib_misc log.c main.c)

target_include_directories(
    glib_misc
    PRIVATE
    /usr/include/glib-2.0
    /usr/lib/x86_64-linux-gnu/glib-2.0/include
)

target_link_libraries(
    glib_misc
    glib-2.0
)

add_executable(childp childp.c)
add_executable(parent parent.c)

add_executable(test_main test_main.c log.c)
target_include_directories(
    test_main
    PRIVATE
    /usr/include/glib-2.0
    /usr/lib/x86_64-linux-gnu/glib-2.0/include
)

target_link_libraries(
    test_main
    glib-2.0
)

add_executable(crashme crashme.c)

target_compile_options(crashme PRIVATE -fsanitize=address)
target_link_libraries(crashme asan)
