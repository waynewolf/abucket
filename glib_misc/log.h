/*
 * 兼容 glib log 的日志系统
 */
#ifndef LOG_H
#define LOG_H

#ifndef G_LOG_DOMAIN
#define G_LOG_DOMAIN (char *)0
#endif

#include <glib.h>

void nc_log(GLogLevelFlags log_level, const char *fmt, ...);
int nc_log_level_pass_filter(GLogLevelFlags log_level, const char *log_domain);

#define NC_DEBUG(fmt, ...) \
    do { \
        if (nc_log_level_pass_filter(G_LOG_LEVEL_DEBUG, G_LOG_DOMAIN)) \
            nc_log(G_LOG_LEVEL_DEBUG, __FILE__ ":%d] " fmt, __LINE__, ##__VA_ARGS__); \
    } while(0)
#define NC_INFO(fmt, ...) \
    do { \
        if (nc_log_level_pass_filter(G_LOG_LEVEL_INFO, G_LOG_DOMAIN)) \
            nc_log(G_LOG_LEVEL_INFO, __FILE__ ":%d] " fmt, __LINE__, ##__VA_ARGS__); \
    } while(0)
#define NC_MESSAGE(fmt, ...) \
    do { \
        if (nc_log_level_pass_filter(G_LOG_LEVEL_MESSAGE, G_LOG_DOMAIN)) \
            nc_log(G_LOG_LEVEL_MESSAGE, __FILE__ ":%d] " fmt, __LINE__, ##__VA_ARGS__); \
    } while(0)
#define NC_WARNING(fmt, ...) \
    do { \
        if (nc_log_level_pass_filter(G_LOG_LEVEL_WARNING, G_LOG_DOMAIN)) \
            nc_log(G_LOG_LEVEL_WARNING, __FILE__ ":%d] " fmt, __LINE__, ##__VA_ARGS__); \
    } while(0)
#define NC_CRITICAL(fmt, ...) \
    do { \
        if (nc_log_level_pass_filter(G_LOG_LEVEL_CRITICAL, G_LOG_DOMAIN)) \
            nc_log(G_LOG_LEVEL_CRITICAL, __FILE__ ":%d] " fmt, __LINE__, ##__VA_ARGS__); \
    } while(0)
#define NC_ERROR(fmt, ...) \
    do { \
        if (nc_log_level_pass_filter(G_LOG_LEVEL_ERROR, G_LOG_DOMAIN)) \
            nc_log(G_LOG_LEVEL_ERROR, __FILE__ ":%d] " fmt, __LINE__, ##__VA_ARGS__); \
    } while(0)

#ifdef g_debug
#undef g_debug
#endif

#ifdef g_info
#undef g_info
#endif

#ifdef g_message
#undef g_message
#endif

#ifdef g_warning
#undef g_warning
#endif

#ifdef g_critical
#undef g_critical
#endif

#ifdef g_error
#undef g_error
#endif


#define g_debug(fmt, ...) NC_DEBUG(fmt, ##__VA_ARGS__)
#define g_info(fmt, ...) NC_INFO(fmt, ##__VA_ARGS__)
#define g_message(fmt, ...) NC_MESSAGE(fmt, ##__VA_ARGS__)
#define g_warning(fmt, ...) NC_WARNING(fmt, ##__VA_ARGS__)
#define g_critical(fmt, ...) NC_CRITICAL(fmt, ##__VA_ARGS__)
#define g_error(fmt, ...) NC_ERROR(fmt, ##__VA_ARGS__)

#endif