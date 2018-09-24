#include <unistd.h>
#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include <sys/time.h>
#include <sys/types.h>
#include <sys/syscall.h>
#include "log.h"

#define MAX_DEBUG_OUTPUT_DOMAINS 16

/*
 * parsed when process is initialized, readonly for the rest of the time,
 * so we don't care about thread safety
 */
static char *debug_output_domains[MAX_DEBUG_OUTPUT_DOMAINS] = { 0 };

static const gchar *log_level_to_string(GLogLevelFlags log_level)
{
    if (log_level & G_LOG_LEVEL_ERROR)
        return "ERROR";
    else if (log_level & G_LOG_LEVEL_CRITICAL)
        return "CRITICAL";
    else if (log_level & G_LOG_LEVEL_WARNING)
        return "WARNING";
    else if (log_level & G_LOG_LEVEL_MESSAGE)
        return "MESSAGE";
    else if (log_level & G_LOG_LEVEL_INFO)
        return "INFO";
    else if (log_level & G_LOG_LEVEL_DEBUG)
        return "DEBUG";

    /* Default to LOG_NOTICE for custom log levels. */
    return "MESSAGE";
}

static const gchar *log_level_to_priority(GLogLevelFlags log_level)
{
    if (log_level & G_LOG_LEVEL_ERROR)
        return "3";
    else if (log_level & G_LOG_LEVEL_CRITICAL)
        return "4";
    else if (log_level & G_LOG_LEVEL_WARNING)
        return "4";
    else if (log_level & G_LOG_LEVEL_MESSAGE)
        return "5";
    else if (log_level & G_LOG_LEVEL_INFO)
        return "6";
    else if (log_level & G_LOG_LEVEL_DEBUG)
        return "7";

    /* Default to LOG_NOTICE for custom log levels. */
    return "5";
}

static FILE *log_level_to_file(GLogLevelFlags log_level)
{
    if (log_level & (G_LOG_LEVEL_ERROR | G_LOG_LEVEL_CRITICAL |
                     G_LOG_LEVEL_WARNING | G_LOG_LEVEL_MESSAGE))
        return stderr;
    else
        return stdout;
}

static const gchar *log_level_to_color(GLogLevelFlags log_level, gboolean use_color)
{
    if (!use_color)
        return "";

    if (log_level & G_LOG_LEVEL_ERROR)
        return "\033[1;31m"; /* red */
    else if (log_level & G_LOG_LEVEL_CRITICAL)
        return "\033[1;35m"; /* magenta */
    else if (log_level & G_LOG_LEVEL_WARNING)
        return "\033[1;33m"; /* yellow */
    else if (log_level & G_LOG_LEVEL_MESSAGE)
        return "\033[1;32m"; /* green */
    else if (log_level & G_LOG_LEVEL_INFO)
        return "\033[1;32m"; /* green */
    else if (log_level & G_LOG_LEVEL_DEBUG)
        return "\033[1;32m"; /* green */

    /* No color for custom log levels. */
    return "";
}

static ssize_t nc_log_time_format(char *buf, size_t sz)
{
    struct timeval tv;
    gettimeofday(&tv, NULL);
    struct tm gm;
    gmtime_r(&tv.tv_sec, &gm);

    ssize_t written = (ssize_t)strftime(buf, sz, "%Y-%m-%dT%H:%M:%S", &gm);
    if ((written > 0) && ((size_t)written < sz)) {
        int w = snprintf(buf + written, sz - (size_t)written, ".%06ldZ", tv.tv_usec);
        written = (w > 0) ? written + w : -1;
    }

    return written;
}

/*
 * This is the behavior of glib log
 */
int nc_log_level_pass_filter(GLogLevelFlags log_level, const char *log_domain)
{
    int ret = 0;

    if (log_level & G_LOG_LEVEL_MESSAGE || log_level & G_LOG_LEVEL_WARNING ||
        log_level & G_LOG_LEVEL_CRITICAL || log_level & G_LOG_LEVEL_ERROR) {
        ret = 1;
    } else {
        if (!log_domain)
            goto out;
        for(int i=0; i< MAX_DEBUG_OUTPUT_DOMAINS && debug_output_domains[i] != NULL; i++) {
            if (!strcmp(debug_output_domains[i], "all")) {
                ret = 1;
                break;
            } else if (!strcmp(log_domain, debug_output_domains[i])) {
                ret = 1;
                break;
            }
        }
    }

    out:
    return ret;
}

void nc_log(GLogLevelFlags log_level, const char *fmt, ...)
{
    char buf[1024];
    char new_format[256];

    va_list args;

    va_start(args, fmt);

    char time_str[28];
    nc_log_time_format(time_str, sizeof(time_str));
    snprintf(new_format, 255, "%s%s %s (%d:%d) %s",
             log_level_to_color(log_level, TRUE),
             log_level_to_string(log_level),
             time_str,
             getpid(),
             (pid_t)syscall(SYS_gettid),
             fmt);

    vsnprintf(buf, 1023, new_format, args);

    va_end(args);

    if (log_level & G_LOG_LEVEL_DEBUG || log_level & G_LOG_LEVEL_INFO || log_level & G_LOG_LEVEL_MESSAGE) {
        printf("%s\n", buf);
    } else if (log_level & G_LOG_LEVEL_WARNING || log_level & G_LOG_LEVEL_CRITICAL || log_level & G_LOG_LEVEL_ERROR) {
        fprintf(stderr, "%s\n", buf);
    }

}

static void stdout_print_handler(const gchar *string)
{
    printf("\033[0m%s", string);
}

static void stderr_print_handler(const gchar *string)
{
    fprintf(stderr, "\033[0m%s", string);
}

__attribute__((constructor)) static void nc_log_init()
{
    g_set_print_handler(stdout_print_handler);
    g_set_printerr_handler(stderr_print_handler);

    char *g_messages_debug_env = getenv("G_MESSAGES_DEBUG");
    if (!g_messages_debug_env)
        return;

    char *token, *saved_ptr;
    int num_domains = 0;
    token = strtok_r(g_messages_debug_env, " ", &saved_ptr);
    /* support MAX_DEBUG_OUTPUT_DOMAINS - 1 debug message domains in G_MESSAGES_DEBUG */
    while (token && num_domains < MAX_DEBUG_OUTPUT_DOMAINS - 1) {
        debug_output_domains[num_domains] = strdup(token);
        token = strtok_r(NULL, " ", &saved_ptr);
        num_domains++;
    }
    debug_output_domains[num_domains] = NULL;
}

__attribute__((destructor)) static void nc_log_fini()
{
    for (int i=0; i<MAX_DEBUG_OUTPUT_DOMAINS && debug_output_domains[i] != NULL; i++) {
        free(debug_output_domains[i]);
    }
}