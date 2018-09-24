#define G_LOG_DOMAIN "test"
#include "log.h"

void some_function_test()
{
    NC_DEBUG("some_function_test");
    g_assert_cmpuint(1, ==, 1);
    g_assert_cmpstr("abc", !=, "def");
}

int main(int argc, char *argv[])
{
    g_test_init(&argc, &argv, NULL);
    g_test_add_func("/some_module/some_function_test", some_function_test);

    return g_test_run();
}