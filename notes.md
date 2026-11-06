references:


when adding logs or creating a new project etc. :
instead of inputting raw into the cli, perhaps open xdg editor scratch buffer
and have that be sent to the program. This should work with neovim, vim, helix
and emacs at least...?

```

status (same for both items and projects):
0: not started
1: in progress
2: on hold
3: finished

delimiter: |
block-start: <%%
block-end: %%>

__pro__<%%id(int)|name(str)|status(int)|created(str ISO)|changed(str ISO)|descr(str)%%>
__lis__<%%created(str ISO)|changed(str ISO)|name(str)|descr(str)%%>
__ite__<%%name(str)|status(int)|text(str)%%>
__log__<%%created(str ISO)|text(str)%%>

```

