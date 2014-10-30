#include <iostream>
#include <fstream>
#include <vector>
#include <algorithm>
#include "FileSystem.h"

int main(int argc, char *argv[]){
	std::string dir = "C:/Users/pshar_000/Downloads/Graph/CodeDependencyBuilder";
	std::vector<std::string> headerFiles = FileSystem::Directory::getFiles(dir, "*.h");
	std::vector<std::string> codeFiles = FileSystem::Directory::getFiles(dir, "*.cpp");

	std::string includeString = "#include ";

	headerFiles.insert(headerFiles.end(), codeFiles.begin(), codeFiles.end());
	for (size_t i = 0; i < headerFiles.size(); i++){
		std::string filePath = dir + "/" + headerFiles[i];
		std::ifstream in_stream;
		in_stream.open(filePath);
		std::string line;

		boolean hasIncludeEncountered = false;
		while (std::getline(in_stream,line)){
			line.erase(line.find_last_not_of(" \n\r\t") + 1);
			int index = line.find(includeString);
			if (index == 0){
				hasIncludeEncountered = true;
				std::string dependency = line.substr(includeString.length());
				if (dependency.find("\"")==0){
					std::string name = dependency.substr(1, dependency.size() - 2);
					std::cout << headerFiles[i] << " - " << name << std::endl;
				}
			}
			else{
				if (line.size() > 0 && hasIncludeEncountered){
					break;
				}
			}
		}
	}
	return 0;
}