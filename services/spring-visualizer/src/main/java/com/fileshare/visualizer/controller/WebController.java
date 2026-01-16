package com.fileshare.visualizer.controller;

import com.fileshare.visualizer.dto.FileInfoDto;
import com.fileshare.visualizer.service.FileMetadataService;
import com.fileshare.visualizer.service.SoapClientService;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestParam;

import java.util.List;

@Slf4j
@Controller
@RequiredArgsConstructor
public class WebController {

    private final SoapClientService soapClientService;
    private final FileMetadataService fileMetadataService;

    @GetMapping("/")
    public String index(Model model) {
        log.info("Web request to show unified dashboard");
        
        List<FileInfoDto> files = fileMetadataService.getAllFiles();
        model.addAttribute("files", files);
        model.addAttribute("totalFiles", files.size());
        
        return "index";
    }

    @GetMapping("/dashboard")
    public String dashboard(Model model) {
        log.info("Web request to show all files dashboard");
        
        List<FileInfoDto> files = fileMetadataService.getAllFiles();
        model.addAttribute("files", files);
        model.addAttribute("totalFiles", files.size());
        
        return "dashboard";
    }

    @GetMapping("/files")
    public String listFiles(@RequestParam String userId, Model model) {
        log.info("Web request to list files for user: {}", userId);
        
        List<FileInfoDto> files = soapClientService.getUserFiles(userId);
        model.addAttribute("files", files);
        model.addAttribute("userId", userId);
        
        return "files";
    }

    @GetMapping("/files/{fileId}")
    public String viewFile(@PathVariable String fileId, Model model) {
        log.info("Web request to view file: {}", fileId);
        
        FileInfoDto file = soapClientService.getFile(fileId);
        model.addAttribute("file", file);
        
        return "file-detail";
    }
}
