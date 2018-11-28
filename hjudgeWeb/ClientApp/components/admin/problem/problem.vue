<template>
    <v-container>
        <v-card>
            <v-tabs right>
                <v-card-title primary-title>
                    <h2>题目编辑</h2>
                </v-card-title>
                <v-spacer></v-spacer>
                <v-tab :key="1">
                    信息
                </v-tab>
                <v-tab :key="2">
                    描述
                </v-tab>
                <v-tab :key="3">
                    数据
                </v-tab>
                <v-tab :key="4">
                    高级
                </v-tab>
                <v-tab :key="5">
                    工具
                </v-tab>
                <v-tab-item :key="1">
                    <v-card-text>
                        <v-container v-if="loading">
                            <p>加载中...</p>
                        </v-container>
                        <v-container v-else>
                            <v-form ref="basic" v-model="valid_basic" lazy-validation>
                                <v-text-field v-model="problem.name"
                                              :rules="requireRules"
                                              label="题目名称 *"
                                              required>
                                </v-text-field>
                                <v-slider v-model="problem.level"
                                          max="10"
                                          min="1"
                                          step="1"
                                          :label="`题目难度：${problem.level}`"
                                          required>
                                </v-slider>
                                <v-layout v-if="problem.type === 1">
                                    <v-checkbox label="使用标准输入输出" v-model="problem.config.useStdIO"></v-checkbox>
                                    <v-text-field v-model="problem.config.inputFileName"
                                                  :rules="requireRules"
                                                  label="输入文件名 *"
                                                  required
                                                  v-if="!problem.config.useStdIO">
                                    </v-text-field>
                                    <v-text-field v-model="problem.config.outputFileName"
                                                  :rules="requireRules"
                                                  label="输出文件名 *"
                                                  required
                                                  v-if="!problem.config.useStdIO">
                                    </v-text-field>
                                </v-layout>
                                <v-radio-group v-model="problem.type" label="题目类型">
                                    <v-layout>
                                        <div>
                                            <v-radio key="1"
                                                     label="提交代码"
                                                     :value="1"></v-radio>
                                        </div>
                                        <div>
                                            <v-radio key="2"
                                                     label="提交答案"
                                                     :value="2"></v-radio>
                                        </div>
                                    </v-layout>
                                </v-radio-group>
                                <v-checkbox label="隐藏题目"
                                            v-model="problem.hidden">
                                </v-checkbox>
                            </v-form>
                        </v-container>
                    </v-card-text>
                </v-tab-item>

                <v-tab-item :key="2">
                    <v-card-text>
                        <v-container v-if="loading">
                            <p>加载中...</p>
                        </v-container>
                        <v-container v-else>
                            <textarea hidden id="editor" v-model="problem.description"></textarea>
                        </v-container>
                    </v-card-text>
                </v-tab-item>

                <v-tab-item :key="3">
                    <v-card-text>
                        <v-container v-if="loading">
                            <p>加载中...</p>
                        </v-container>
                        <v-container v-else-if="problem.type === 1">
                            <v-layout>
                                <v-tooltip bottom>
                                    <v-btn icon @click="addPoint" slot="activator"><v-icon color="primary">add</v-icon></v-btn>
                                    <span>添加</span>
                                </v-tooltip>
                                <v-combobox v-model="templateSelection"
                                            label="快速套用模板"
                                            :items="dataTemplate">
                                </v-combobox>
                                <v-tooltip bottom>
                                    <v-btn icon @click="applyTemplate" slot="activator"><v-icon color="primary">done</v-icon></v-btn>
                                    <span>应用</span>
                                </v-tooltip>
                            </v-layout>
                            <v-form v-model="valid_points" ref="points" lazy-validation>
                                <template v-for="(item, index) in problem.config.points">
                                    <v-layout :key="index">
                                        <h4>数据点 #{{index + 1}}</h4>
                                        <v-spacer></v-spacer>
                                        <v-tooltip bottom>
                                            <v-btn @click="removePoint(index)" icon slot="activator"><v-icon color="red">delete</v-icon></v-btn>
                                            <span>删除</span>
                                        </v-tooltip>
                                    </v-layout>
                                    <v-layout :key="index">
                                        <v-text-field v-model="item.stdInFile" label="标准输入文件 *" :rules="requireRules" required>
                                        </v-text-field>
                                        <v-text-field v-model="item.stdOutFile" label="标准输出文件 *" :rules="requireRules" required>
                                        </v-text-field>
                                    </v-layout>
                                    <v-layout :key="index">
                                        <v-text-field v-model="item.timeLimit" label="时间限制 (ms) *" type="number" :rules="requireRules" required>
                                        </v-text-field>
                                        <v-text-field v-model="item.memoryLimit" label="内存限制 (kb) *" type="number" :rules="requireRules" required>
                                        </v-text-field>
                                        <v-text-field v-model="item.score" label="分数 *" type="number" :rules="requireRules" required>
                                        </v-text-field>
                                    </v-layout>
                                </template>
                            </v-form>
                        </v-container>
                        <v-container v-else>
                            <v-form v-model="valid_answer" ref="answer" lazy-validation>
                                <v-text-field v-model="problem.config.answer.answerFile"
                                              :rules="requireRules"
                                              label="答案文件 *"
                                              required>
                                </v-text-field>
                                <v-text-field v-model="problem.config.answer.score"
                                              :rules="requireRules"
                                              label="分数 *"
                                              type="number"
                                              required>
                                </v-text-field>
                            </v-form>
                        </v-container>
                    </v-card-text>
                </v-tab-item>

                <v-tab-item :key="4">
                    <v-card-text>
                        <v-container v-if="loading">
                            <p>加载中...</p>
                        </v-container>
                        <v-container v-else>
                            <v-radio-group label="默认比较选项" v-if="!problem.config.specialJudge">
                                <v-layout>
                                    <div>
                                        <v-checkbox v-model="problem.config.comparingOptions.ignoreLineTailWhiteSpaces" label="忽略行末空格"></v-checkbox>
                                    </div>
                                    <div>
                                        <v-checkbox v-model="problem.config.comparingOptions.ignoreTextTailLineFeeds" label="忽略文末空行"></v-checkbox>
                                    </div>
                                </v-layout>
                            </v-radio-group>
                            <v-text-field label="自定义比较程序" v-model="problem.config.specialJudge"></v-text-field>
                            <v-textarea outline v-model="problem.config.extraFilesText" label="附加文件"></v-textarea>
                            <v-textarea v-if="problem.type === 1"
                                        outline
                                        label="自定义编译参数（格式：[语言]参数）"
                                        v-model="problem.config.compileArgs">
                            </v-textarea>
                            <v-text-field label="自定义提交文件名 (不含扩展名)" v-model="problem.config.submitFileName"></v-text-field>
                            <v-text-field label="提交语言限制（以 ; 分隔）" v-model="problem.config.languages"></v-text-field>
                        </v-container>
                    </v-card-text>
                </v-tab-item>

                <v-tab-item :key="5">
                    <v-card-text>
                        <v-container v-if="loading">
                            <p>加载中...</p>
                        </v-container>
                        <v-container v-else-if="problem.id === 0">
                            <p>请先保存题目</p>
                        </v-container>
                        <v-container v-else>
                            <v-tooltip bottom>
                                <v-btn color="primary" slot="activator" @click="selectFile" :disabled="uploading">{{uploadingText}}</v-btn>
                                <span>仅限 .zip 文件，大小不超过 128 Mb</span>
                            </v-tooltip>
                            <input type="file" id="data_file" @change="uploadData" accept="application/x-zip-compressed,application/zip" style="filter: alpha(opacity=0); opacity: 0; width: 0; height: 0;" />
                            <v-btn color="primary" @click="downloadData">下载题目数据</v-btn>
                            <v-btn color="red" @click="deleteData">删除题目数据</v-btn>
                        </v-container>
                    </v-card-text>
                </v-tab-item>
            </v-tabs>

            <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn color="primary" :disabled="!valid() || submitting || markdownEnabled" @click="save">保存</v-btn>
            </v-card-actions>
        </v-card>
    </v-container>
</template>

<script type="text/javascript" src="./problem.js"></script>