/*
Copyright (c) 2010-2019, Mark Final
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

* Neither the name of BuildAMation nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

#ifdef __MINGW32__
// Note: doesn't link against VisualC Qt libraries
#undef __STRICT_ANSI__ // allows __argc and __argc to be define
#include <stdlib.h>
#endif

#include "QtUiTools/QUiLoader"
#include "QtWidgets/QApplication"
#include "QtWidgets/QWidget"
#include "QtCore/QFile"

#if defined(_WINDOWS)
#include <Windows.h>

int APIENTRY WinMain(
  HINSTANCE hInstance,
  HINSTANCE hPrevInstance,
  LPSTR lpCmdLine,
  int nCmdShow
)
#else
int main(int argc, char *argv[])
#endif
{
#if defined(_WINDOWS)
    QApplication app(__argc, __argv);
#else
    QApplication app(argc, argv);
#endif

    QFile ui(":/mainwindow.ui");
    ui.open(QFile::ReadOnly);
    QUiLoader loader;
    QWidget *main_window = loader.load(&ui, 0);
    ui.close();
    //QMetaObject::connectSlotsByName(main_window);

    app.setQuitOnLastWindowClosed(true);
    main_window->setAttribute(Qt::WA_QuitOnClose);

    main_window->show();

    return app.exec();
}
