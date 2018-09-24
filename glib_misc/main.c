#define G_LOG_DOMAIN "main"
#include "log.h"
#include <glib.h>

void test_log_level()
{
    g_debug("I'm g_debug");
    g_info("I'm g_info, %s", "it works");
    g_message("I'm g_message %d", 1);
    g_warning("I'm g_warning");
    g_critical("I'm g_critical");
    //g_error("I'm g_error");
}

void test_my_log_level()
{
    NC_DEBUG("In NC_DEBUG");
    NC_INFO("In NC_INFO, %s", "it works");
    NC_MESSAGE("In NC_MESSAGE %d", 1);
    NC_WARNING("In NC_WARNING");
    NC_CRITICAL("In NC_CRITICAL");
    NC_ERROR("In NC_ERROR");
}

void test_g_print()
{
    g_print("g_print %s\n", "aaa");
    g_printerr("g_printerr %d\n", 111);
}

int main()
{
    test_log_level();
    test_my_log_level();
    test_g_print();

    return 0;
}