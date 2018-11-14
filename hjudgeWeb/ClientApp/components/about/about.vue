<template>
    <v-container>
        <v-card>
            <v-card-title primary-title>
                <p>
                    <h2>关于 H::Judge</h2>
                    <v-spacer></v-spacer>
                    这是你从来没有上过的船新 H::Judge，快来体验吧
                </p>
            </v-card-title>
            <v-card-text>
                <p>开发者：hez2010，Email：<a href="mailto:hez2010@126.com">hez2010@126.com</a></p>
                <v-container>
                    <h3>进化历史</h3>
                    <v-timeline align-top dense>
                        <v-timeline-item v-for="(log, i) in logs"
                                         :color="log.color"
                                         :key="i"
                                         small>
                            <v-layout pt-3>
                                <v-flex xs3>
                                    <div :class="`headline font-weight-light mb-3 ${log.color}--text`">{{log.version}}</div>
                                    <strong :class="`${log.color}--text`">{{log.reldate}}</strong>
                                </v-flex>
                                <v-flex>
                                    <div class="py-3">
                                        <div v-for="content in log.content">{{content}}</div>
                                    </div>
                                </v-flex>
                            </v-layout>
                        </v-timeline-item>
                    </v-timeline>
                </v-container>
            </v-card-text>
        </v-card>
    </v-container>
</template>

<script type="text/javascript" src="./about.js"></script>