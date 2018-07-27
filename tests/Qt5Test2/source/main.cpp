// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#include <QtCore/QCoreApplication>
#include <QtCore/QLibraryInfo>

#include <QtCore/QFile>
#include <QtCore/QTextStream>
#include <QtCore/QDebug>

#include <iostream>

int main(int argc, char *argv[])
{
    QCoreApplication app(argc, argv);

    QString appDirPath = QCoreApplication::applicationDirPath();
    QString prefix = QLibraryInfo::location(QLibraryInfo::PrefixPath);
    if (!appDirPath.startsWith(prefix))
    {
        std::cerr << "Mis-configured distribution: prefix does not match the application dir" << std::endl;
        std::cerr << "Prefix : " << qPrintable(prefix) << std::endl;
        std::cerr << "AppDir : " << qPrintable(appDirPath) << std::endl;
        return -1;
    }
    std::cout << "Plugin path: '" << qPrintable(QLibraryInfo::location(QLibraryInfo::PluginsPath)) << "'" << std::endl;

    QFile embedded(":/embedded.txt");
    if (!embedded.open(QFile::ReadOnly | QFile::Text))
    {
        return -1;
    }
    QTextStream in(&embedded);
    QString text = in.readAll();
    qDebug() << text;
    embedded.close();

    return 0;
}
