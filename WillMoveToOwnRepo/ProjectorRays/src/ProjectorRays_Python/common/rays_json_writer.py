import json

class RaysJSONWriter:
    def __init__(self):
        self._stack = []
        self._root = None
        self._current_key = None

    def start_object(self):
        obj = {}
        if self._stack:
            parent = self._stack[-1]
            if isinstance(parent, list):
                parent.append(obj)
            elif isinstance(parent, dict):
                parent[self._current_key] = obj
                self._current_key = None
        else:
            self._root = obj
        self._stack.append(obj)

    def end_object(self):
        if self._stack and isinstance(self._stack[-1], dict):
            self._stack.pop()

    def start_array(self):
        arr = []
        if self._stack:
            parent = self._stack[-1]
            if isinstance(parent, dict):
                parent[self._current_key] = arr
                self._current_key = None
            elif isinstance(parent, list):
                parent.append(arr)
        else:
            self._root = arr
        self._stack.append(arr)

    def end_array(self):
        if self._stack and isinstance(self._stack[-1], list):
            self._stack.pop()

    def write_key(self, key):
        self._current_key = key

    def write_val(self, val):
        parent = self._stack[-1]
        if isinstance(parent, dict):
            parent[self._current_key] = val
            self._current_key = None
        elif isinstance(parent, list):
            parent.append(val)

    def to_string(self, indent=2):
        return json.dumps(self._root, indent=indent)
