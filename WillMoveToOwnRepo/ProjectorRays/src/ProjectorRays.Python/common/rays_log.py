import sys

class RaysLog:
    """Minimal logging utilities."""

    verbose: bool = False

    @staticmethod
    def write(msg: str):
        print(msg)

    @staticmethod
    def debug(msg: str):
        if RaysLog.verbose:
            RaysLog.write(msg)

    @staticmethod
    def warning(msg: str):
        sys.stderr.write(msg + '\n')
