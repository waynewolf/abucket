#include <unistd.h>
#include <stdio.h>
#include <stdlib.h>
#include <signal.h>
#include <backtrace.h>
#include <execinfo.h>
#include <memory.h>
#include <malloc.h>

/* Prototypes for our hooks.  */
//static void my_memhook_init(void);
//static void *my_malloc_hook (size_t, const void *);
//static void my_free_hook (void*, const void *);
//
//void *(*__MALLOC_HOOK_VOLATILE old_malloc_hook)(size_t __size, const void *);
//void (*__MALLOC_HOOK_VOLATILE old_free_hook)(void *__ptr, const void *);
//
//static void my_memhook_init(void)
//{
//    old_malloc_hook = __malloc_hook;
//    old_free_hook = __free_hook;
//    __malloc_hook = my_malloc_hook;
//    __free_hook = my_free_hook;
//}
//
//static void *my_malloc_hook(size_t size, const void *caller) {
//    void *result;
//    /* Restore all old hooks */
//    __malloc_hook = old_malloc_hook;
//    __free_hook = old_free_hook;
//    /* Call recursively */
//    result = malloc (size);
//    /* Save underlying hooks */
//    old_malloc_hook = __malloc_hook;
//    old_free_hook = __free_hook;
//    /* printf might call malloc, so protect it too. */
//    printf ("malloc (%u) returns %p\n", (unsigned int) size, result);
//    /* Restore our own hooks */
//    __malloc_hook = my_malloc_hook;
//    __free_hook = my_free_hook;
//    return result;
//}
//
//static void my_free_hook(void *ptr, const void *caller)
//{
//    /* Restore all old hooks */
//    __malloc_hook = old_malloc_hook;
//    __free_hook = old_free_hook;
//    /* Call recursively */
//    free (ptr);
//    /* Save underlying hooks */
//    old_malloc_hook = __malloc_hook;
//    old_free_hook = __free_hook;
//    /* printf might call free, so protect it too. */
//    printf ("freed pointer %p\n", ptr);
//    /* Restore our own hooks */
//    __malloc_hook = my_malloc_hook;
//    __free_hook = my_free_hook;
//}


void abort_me(int countdown)
{
    if (countdown == 0) {
        abort();
    } else {
        abort_me(--countdown);
    }
}

void crash_me(int countdown)
{
    if (countdown == 0) {
        *(int *)countdown = 'a';
    } else {
        crash_me(--countdown);
    }
}

void print_trace (void)
{
    void *array[20];
    size_t size;
    char **strings;
    size_t i;

    size = backtrace(array, 20);
    strings = backtrace_symbols(array, size);
    if (strings == NULL) {
        perror("backtrace_synbols");
        exit(EXIT_FAILURE);
    }

    printf("Obtained %zd stack frames.\n", size);

    for(i = 0; i < size; i++)
        printf("%s\n", strings[i]);

    free(strings);
    strings = NULL;
}

void sig_handler(int signo)
{
    print_trace();
    exit(EXIT_FAILURE);
}

void do_something_then_free_ptr(int countdown, char *p)
{
    sleep(3);
    if (--countdown <= 0) {
        free(p);
    } else {
        do_something_then_free_ptr(countdown, p);
    }
}

void double_free()
{
    char *p = (char *)malloc(4);

    p[0] = 'b';
    p[1] = 'e';
    p[2] = 'a';
    p[3] = 'f';

    free(p);
    do_something_then_free_ptr(2, p);

    p[0] = 'd';
    p[1] = 'e';
    p[2] = 'a';
    p[3] = 'd';
}

void overflow()
{
    char *p = (char *)malloc(4);
    strcpy(p, "hello world");
    free(p);

    printf("%s\n", p);
}

int main(int argc, char *argv[])
{
    signal(SIGSEGV, sig_handler);
    signal(SIGABRT, sig_handler);

    //crash_me(10);
    //abort_me(10);

    //my_memhook_init();

    overflow();

    double_free();

    return 0;
}