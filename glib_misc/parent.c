#include <unistd.h>
#include <stdlib.h>
#include <stdio.h>
#include <sys/types.h>
#include <sys/wait.h>
#include <errno.h>
#include <string.h>

int main(int argc, char *argv[])
{
    char *child_argv[] = {
        "childp", /* 注意，这个地方需要加一个程序名本身，这样才和直接调用子程序一致 */
        "hello",
        "darling",
        NULL
    };

    int child = fork();
    if (child == 0) {
        int ret = execvp("/home/waywu01/work/abucket/glib_misc/cmake-build-debug/childp", child_argv);
        if (ret == -1)
            printf("execvp failed because: %s\n", strerror(errno));
        exit(0);
    }

    int wstatus;
    wait(&wstatus);

    return 0;
}