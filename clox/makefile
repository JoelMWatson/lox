IDIR=includes
CC=gcc
CFLAGS=-I $(IDIR)

ODIR=obj

_DEPS = common.h chunk.h memory.h debug.h value.h vm.h compiler.h scanner.h object.h table.h
DEPS = $(patsubst %,$(IDIR)/%,$(_DEPS))

_OBJ = main.o chunk.o memory.o debug.o value.o vm.o compiler.o scanner.o object.o table.o
OBJ = $(patsubst %,$(ODIR)/%,$(_OBJ))

$(ODIR)/%.o: src/%.c $(DEPS)
	$(CC) -c -o $@ $< $(CFLAGS)

lox: $(OBJ)
	$(CC) -o $@ $^ $(CFLAGS)  

.PHONY: clean

clean:
	rm -f $(ODIR)/*.o *~ core $(IDIR)/*~ 