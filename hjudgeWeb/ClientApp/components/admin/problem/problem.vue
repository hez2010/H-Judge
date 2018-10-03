<template>
    <v-container>
        <v-card>
            <v-card-title primary-title>
                <h2>题目编辑</h2>
            </v-card-title>
            <v-card-text>
                <textarea hidden id="editor">
                    This is my textarea to be replaced with CKEditor.
                </textarea>
            </v-card-text>
        </v-card>
        <remote-script src="/lib/ckeditor/ckeditor.js"></remote-script>
    </v-container>
</template>

<script type="text/javascript" src="./problem.js"></script>