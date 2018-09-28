<template>
    <div>
        <section>
            <v-parallax src="assets/hero.jpeg" height="600">
                <v-layout column align-center justify-center class="white--text">
                    <img src="assets/hjudge.png" alt="H::Judge" height="200">

                    <v-btn class="blue lighten-2 mt-5" dark large to="/Problem">
                        进入题库
                    </v-btn>
                </v-layout>
            </v-parallax>
        </section>

        <section>
            <v-layout column
                      wrap
                      class="my-5"
                      align-center>
                <v-flex xs12 sm4 class="my-3">
                    <div class="text-xs-center">
                        <h2 class="headline">H::Judge</h2>
                        <span class="subheading">
                            为你量身定做的评测系统
                        </span>
                    </div>
                </v-flex>
                <v-flex xs12>
                    <v-container grid-list-xl>
                        <v-layout row wrap align-center>
                            <v-flex xs12 md4>
                                <v-card class="elevation-0 transparent">
                                    <v-card-text class="text-xs-center">
                                        <v-icon x-large class="blue--text text--lighten-2">color_lens</v-icon>
                                    </v-card-text>
                                    <v-card-title primary-title class="layout justify-center">
                                        <div class="headline text-xs-center">Material Design</div>
                                    </v-card-title>
                                    <v-card-text>
                                        重新设计的 H::Judge，采用 Material Design 风格设计，界面美观却不失操作的便捷性。
                                    </v-card-text>
                                </v-card>
                            </v-flex>
                            <v-flex xs12 md4>
                                <v-card class="elevation-0 transparent">
                                    <v-card-text class="text-xs-center">
                                        <v-icon x-large class="blue--text text--lighten-2">flash_on</v-icon>
                                    </v-card-text>
                                    <v-card-title primary-title class="layout justify-center">
                                        <div class="headline">Fast Judge</div>
                                    </v-card-title>
                                    <v-card-text>
                                        H::Judge 使用了高速的评测机和评测算法，可以让每一个提交都能在短时间内得到评测结果。
                                    </v-card-text>
                                </v-card>
                            </v-flex>
                            <v-flex xs12 md4>
                                <v-card class="elevation-0 transparent">
                                    <v-card-text class="text-xs-center">
                                        <v-icon x-large class="blue--text text--lighten-2">build</v-icon>
                                    </v-card-text>
                                    <v-card-title primary-title class="layout justify-center">
                                        <div class="headline text-xs-center">Study Assistant</div>
                                    </v-card-title>
                                    <v-card-text>
                                        H::Judge 中丰富的题目和比赛将助力你的程序设计学习/比赛生涯，快速提升你的编程水平。
                                    </v-card-text>
                                </v-card>
                            </v-flex>
                        </v-layout>
                    </v-container>
                </v-flex>
            </v-layout>
        </section>

        <section>
            <v-parallax src="assets/section.jpg" height="380">
                <v-layout column align-center justify-center>
                    <div class="headline white--text mb-3 text-xs-center">H::Judge 每天都在进步</div>
                    <p>更新日志中记录了 H::Judge 的每一次更新内容，每一次的更新都是为了带来更好的体验</p>
                    <v-btn class="blue lighten-2 mt-5" dark large to="/About">
                        了解详情
                    </v-btn>
                </v-layout>
            </v-parallax>
        </section>

        <section>
            <v-container grid-list-xl>
                <v-layout row wrap justify-center class="my-5">
                    <v-flex xs12 sm4>
                        <v-card class="elevation-0 transparent">
                            <v-card-title primary-title class="layout justify-center">
                                <div class="headline">关于 H::Judge</div>
                            </v-card-title>
                            <v-card-text>
                                H::Judge 的前身为 hjudge，经重新设计后稳定性、功能性和速度都有了大幅提升。欢迎体验！
                            </v-card-text>
                        </v-card>
                    </v-flex>
                    <v-flex xs12 sm4 offset-sm1>
                        <v-card class="elevation-0 transparent">
                            <v-card-title primary-title class="layout justify-center">
                                <div class="headline">联系方式</div>
                            </v-card-title>
                            <v-card-text>
                                如在使用中遇到任何问题，请联系。
                            </v-card-text>
                            <v-list class="transparent">
                                <v-list-tile>
                                    <v-list-tile-action>
                                        <v-icon class="blue--text text--lighten-2">email</v-icon>
                                    </v-list-tile-action>
                                    <v-list-tile-content>
                                        <v-list-tile-title>hez2010@126.com</v-list-tile-title>
                                    </v-list-tile-content>
                                </v-list-tile>
                            </v-list>
                        </v-card>
                    </v-flex>
                </v-layout>
            </v-container>
        </section>
    </div>
</template>

<script src="./home.js"></script>