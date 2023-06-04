import os
import subprocess
import time
from typing import IO


# build_mode = os.environ["BuildMode"]
# release_version = os.environ["ReleaseVersion"]


def call(cmd: str) -> IO[str] | None:
    pop = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, universal_newlines=True,
                           encoding="utf-8")
    return pop.stdout


def switch_branch():
    pass


def get_head_sha():
    git_cmd = "git rev-parse --short HEAD "
    o = call(git_cmd)
    return o.readline()


def call_with_return(cmd: str):
    o = call(cmd)
    return o.readline()

if __name__ == '__main__':
    build_cmd = "C:\Program Files\dotnet\dotnet.exe publish ./FastSocket --configuration Release"
    get_cwd = os.getcwd()
    print(f"current working dir: {get_cwd}")
    git_cur_branch_sha = get_head_sha()
    print(f"current git branch: <sha>:{git_cur_branch_sha.strip()} <commit_msg>:{call_with_return('git log -1 HEAD --pretty=%B')}")

    # if build_mode == "Dev":
    #     time_sign = time.strftime("%y%m%d", time.localtime())
    #     sha = get_head_sha()
    #     build_cmd += f" -p:Suffix=build{time_sign}+{sha}"
    # if build_mode == "Release":
    #     # 切tag
    #     git_check_tag_cmd = f"git checkout {release_version}"
    #     i = call(git_check_tag_cmd)
    #     for line in i:
    #         print(line)

    print(f"build cmd: {build_cmd}")
    build_std_out = call(build_cmd)
    for line in build_std_out:
        print(line)
