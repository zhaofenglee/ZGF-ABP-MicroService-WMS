<template>
  <div class="container">
    <div class="left-board">
      <el-scrollbar class="left-scrollbar">
        <div class="components-list">
          <div class="components-title">
            <svg-icon icon-class="input" />输入型组件
          </div>
          <draggable
            class="components-draggable"
            :list="inputComponents"
            :group="{ name: 'componentsGroup', pull: 'clone', put: false }"
            :clone="cloneComponent"
            draggable=".components-item"
            :sort="false"
            @end="onEnd"
          >
            <div
              v-for="(element, index) in inputComponents"
              :key="index"
              class="components-item"
              @click="addComponent(element)"
            >
              <div class="components-body">
                <svg-icon :icon-class="element.fieldType" />
                {{ element.label }}
              </div>
            </div>
          </draggable>
          <div class="components-title">
            <svg-icon icon-class="select" />选择型组件
          </div>
          <draggable
            class="components-draggable"
            :list="selectComponents"
            :group="{ name: 'componentsGroup', pull: 'clone', put: false }"
            :clone="cloneComponent"
            draggable=".components-item"
            :sort="false"
            @end="onEnd"
          >
            <div
              v-for="(element, index) in selectComponents"
              :key="index"
              class="components-item"
              @click="addComponent(element)"
            >
              <div class="components-body">
                <svg-icon :icon-class="element.fieldType" />
                {{ element.label }}
              </div>
            </div>
          </draggable>
          <!-- <div class="components-title">
            <svg-icon icon-class="row" /> 布局型组件
          </div>
          <draggable
            class="components-draggable"
            :list="layoutComponents"
            :group="{ name: 'componentsGroup', pull: 'clone', put: false }"
            :clone="cloneComponent"
            draggable=".components-item"
            :sort="false"
            @end="onEnd"
          >
            <div
              v-for="(element, index) in layoutComponents"
              :key="index"
              class="components-item"
              @click="addComponent(element)"
            >
              <div class="components-body">
                <svg-icon :icon-class="element.fieldType" />
                {{ element.label }}
              </div>
            </div>
          </draggable> -->
          <div class="components-title">
            <svg-icon icon-class="row" /> 引用型组件
          </div>
           <draggable
            class="components-draggable"
            :list="importComponents"
            :group="{ name: 'componentsGroup', pull: 'clone', put: false }"
            :clone="cloneComponent"
            draggable=".components-item"
            :sort="false"
            @end="onEnd"
          >
            <div
              v-for="(element, index) in importComponents"
              :key="index"
              class="components-item"
              @click="addComponent(element)"
            >
              <div class="components-body">
                <svg-icon :icon-class="element.fieldType" />
                {{ element.label }}
              </div>
            </div>
          </draggable>
        </div>
      </el-scrollbar>
    </div>

    <div class="center-board">
      <div class="action-bar">
        <el-button icon="el-icon-download" type="text" @click="download">
          导出vue文件
        </el-button>
        <el-button
          class="save-btn"
          type="text"
          @click="save"
          v-loading.fullscreen.lock="fullscreenLoading"
        >
          <svg-icon icon-class="save" />
          保存
        </el-button>
        <el-button
          class="delete-btn"
          icon="el-icon-delete"
          type="text"
          @click="empty"
        >
          清空
        </el-button>
      </div>
      <el-scrollbar class="center-scrollbar">
        <el-row class="center-board-row" :gutter="formConf.gutter">
          <el-form
            :size="formConf.size"
            :label-position="formConf.labelPosition"
            :disabled="formConf.disabled"
            :label-width="formConf.labelWidth + 'px'"
          >
            <draggable
              class="drawing-board"
              :list="drawingList"
              :animation="340"
              group="componentsGroup"
            >
              <draggable-item
                v-for="(element, index) in drawingList"
                :key="element.renderKey"
                :drawing-list="drawingList"
                :element="element"
                :index="index"
                :active-id="activeId"
                :form-conf="formConf"
                @activeItem="activeFormItem"
                @copyItem="drawingItemCopy"
                @deleteItem="drawingItemDelete"
              />
            </draggable>
            <div v-show="!drawingList.length" class="empty-info">
              从左侧拖入或点选组件进行表单设计
            </div>
          </el-form>
        </el-row>
      </el-scrollbar>
    </div>

    <right-panel
      :active-data="activeData"
      :form-conf="formConf"
      :show-field="!!drawingList.length"
      @tag-change="tagChange"
    />

    <input id="copyNode" type="hidden" />
  </div>
</template>

<script>
import draggable from "vuedraggable";
import { saveAs } from "file-saver";
import beautifier from "js-beautify";
import ClipboardJS from "clipboard";
import render from "@/utils/generator/render";
import RightPanel from "./RightPanel";
import {
  inputComponents,
  selectComponents,
  layoutComponents,
  importComponents,
  formConf,
} from "@/utils/generator/config";
import {
  exportDefault,
  beautifierConf,
  isNumberStr,
  titleCase,
} from "@/utils/index";
import {
  makeUpHtml,
  vueTemplate,
  vueScript,
  cssStyle,
} from "@/utils/generator/html";
import { makeUpJs } from "@/utils/generator/js";
import { makeUpCss } from "@/utils/generator/css";
import drawingDefalut from "@/utils/generator/drawingDefalut";
import CodeTypeDialog from "./CodeTypeDialog";
import DraggableItem from "./DraggableItem";

const emptyActiveData = { style: {}, autosize: {} };
let oldActiveId;
let tempActiveData;

export default {
  name: "FormDetail",
  components: {
    draggable,
    render,
    RightPanel,
    CodeTypeDialog,
    DraggableItem,
  },
  props: {
    isEdit: {
      type: Boolean,
      default: false,
    },
  },
  data() {
    return {
      idGlobal: 100,
      //formConf,
      formConf: Object.assign({}, formConf),
      inputComponents,
      selectComponents,
      layoutComponents,
      importComponents,
      labelWidth: 100,
      drawingList: [],
      drawingData: {},
      activeId: null,
      drawerVisible: false,
      formData: {},
      generateConf: null,
      activeData: {},
      fullscreenLoading: false,
    };
  },
  computed: {},
  watch: {
    // eslint-disable-next-line func-names
    "activeData.label": function (val, oldVal) {
      if (
        this.activeData.placeholder === undefined ||
        !this.activeData.tag ||
        oldActiveId !== this.activeId
      ) {
        return;
      }
      this.activeData.placeholder =
        this.activeData.placeholder.replace(oldVal, "") + val;
    },
    activeId: {
      handler(val) {
        oldActiveId = val;
      },
      immediate: true,
    },
  },
  created() {
    if (this.isEdit) {
      const id = this.$route.params && this.$route.params.id;
      this.drawingList = [];
      this.fetchData(id);
    } else {
      this.formConf = Object.assign({}, formConf);
    }
  },
  methods: {
    fetchData(id) {
      this.$axios.gets("/api/business/form/" + id).then((response) => {
        //TODO：优化response结构
        this.formConf.id = response.id;
        this.formConf.formName = response.formName;
        this.formConf.displayName = response.displayName;
        this.formConf.api = response.api;
        this.formConf.disabled = response.disabled;
        this.formConf.description = response.description;
        response.fields.forEach((item) => {
          let field={}
          let clone = inputComponents.find(
            (_) => _.fieldType == item.fieldType
          );
          if(!clone) {
            return
          }
          field.formId = item.id;
          field.fieldName = item.fieldName;
          field.fieldType = item.fieldType;
          field.label = item.label;
          field.fieldOrder=item.fieldOrder
          field.placeholder = item.placeholder;
          field.defaultValue = item.defaultValue;
          field.icon = item.icon;
          field.maxlength = item.maxlength;
          field.isReadonly = item.isReadonly;
          field.isRequired = item.isRequired;
          field.isSort = item.isSort;
          field.disabled = item.disabled;
          field.regx=clone.regx
          field.span=item.span
          field.options=item.options
          field.tag = clone.tag
          field.style = clone.style
          field.clearable = clone.clearable
          field.prepend = clone.prepend
          field.append = clone.append
          field.changeTag = clone.changeTag
          //clone.renderKey = +new Date();
          if (!clone.layout) field.layout = "colFormItem";
          this.drawingList.push(field);
        });
        
        response.fields.forEach((item) => {
          let field={}
          let clone = selectComponents.find(
            (_) => _.fieldType == item.fieldType
          );
          if(!clone) {
            return
          }
          field.formId = item.id;
          field.fieldName = item.fieldName;
          field.fieldType = item.fieldType;
          field.label = item.label;
          field.fieldOrder=item.fieldOrder
          field.placeholder = item.placeholder;
          field.defaultValue = item.defaultValue;
          field.icon = item.icon;
          field.maxlength = item.maxlength;
          field.isReadonly = item.isReadonly;
          field.isRequired = item.isRequired;
          field.isSort = item.isSort;
          field.disabled = item.disabled;
          field.regx=clone.regx
          field.options=item.options
          field.span = item.span;
          field.tag = clone.tag
          field.style = clone.style
          field.clearable = clone.clearable
          field.prepend = clone.prepend
          field.append = clone.append
          field.changeTag = clone.changeTag
          if (!clone.layout) field.layout = "colFormItem";
          this.drawingList.push(field);
        });
        this.activeId=this.drawingList[0].formId
        this.activeData= this.drawingList[0]
      });
    },
    activeFormItem(element) {
      this.activeData = element;
      this.activeId = element.formId;
    },
    onEnd(obj, a) {
      if (obj.from !== obj.to) {
        this.activeData = tempActiveData;
        this.activeId = this.idGlobal;
      }
    },
    addComponent(item) {
      const clone = this.cloneComponent(item);
      this.drawingList.push(clone);
      this.activeFormItem(clone);
    },
    cloneComponent(origin) {
      const clone = JSON.parse(JSON.stringify(origin));
      clone.formId = ++this.idGlobal;
      clone.span = formConf.span;
      clone.renderKey = +new Date(); // 改变renderKey后可以实现强制更新组件
      if (!clone.layout) clone.layout = "colFormItem";
      if (clone.layout === "colFormItem") {
        clone.fieldName = `field${this.idGlobal}`;
        clone.placeholder !== undefined && (clone.placeholder += clone.label);
        tempActiveData = clone;
      } else if (clone.layout === "rowFormItem") {
        delete clone.label;
        clone.componentName = `row${this.idGlobal}`;
        clone.gutter = this.formConf.gutter;
        tempActiveData = clone;
      }
      return tempActiveData;
    },
    AssembleFormData() {
      this.formData = {
        fields: JSON.parse(JSON.stringify(this.drawingList)),
        ...this.formConf,
      };
    },
    execRun() {
      this.AssembleFormData();
      this.drawerVisible = true;
    },
    execDownload() {
      const codeStr = this.generateCode();
      const blob = new Blob([codeStr], { type: "text/plain;charset=utf-8" });
      saveAs(blob, "index.vue");
    },
    empty() {
      this.$confirm("确定要清空所有组件吗？", "提示", { type: "warning" }).then(
        () => {
          this.drawingList = [];
        }
      );
    },
    save() {
      this.AssembleFormData();
      console.log(JSON.parse(JSON.stringify(this.formData)))
      if (this.formData.formName == "") {
        this.$message({
          message: "表单名为空",
          type: "warning",
        });
        return;
      }
      this.fullscreenLoading = true;

      if (this.isEdit) {
        this.$axios
          .puts("/api/business/form/" + this.formData.id, this.formData)
          .then((response) => {
            this.fullscreenLoading = false;
            this.$notify({
              title: "成功",
              message: "更新成功",
              type: "success",
              duration: 60000,
            });
            this.jump();
          })
          .catch(() => {
            this.fullscreenLoading = false;
          });
      } else {
        this.$axios
          .posts("/api/business/form", this.formData)
          .then((response) => {
            this.fullscreenLoading = false;
            this.$notify({
              title: "成功",
              message: "新增成功",
              type: "success",
              duration: 60000,
            });
            this.jump();
          })
          .catch(() => {
            this.fullscreenLoading = false;
          });
      }
    },
    jump() {
      this.$store.dispatch("tagsView/delView", this.$route);
      this.$router.push({ path: "/tool/form" });
    },
    drawingItemCopy(item, parent) {
      let clone = JSON.parse(JSON.stringify(item));
      clone = this.createIdAndKey(clone);
      parent.push(clone);
      this.activeFormItem(clone);
    },
    createIdAndKey(item) {
      item.formId = ++this.idGlobal;
      item.renderKey = +new Date();
      if (item.layout === "colFormItem") {
        item.fieldName = `field${this.idGlobal}`;
      } else if (item.layout === "rowFormItem") {
        item.componentName = `row${this.idGlobal}`;
      }
      if (Array.isArray(item.children)) {
        item.children = item.children.map((childItem) =>
          this.createIdAndKey(childItem)
        );
      }
      return item;
    },
    drawingItemDelete(index, parent) {
      parent.splice(index, 1);
      this.$nextTick(() => {
        const len = this.drawingList.length;
        if (len) {
          this.activeFormItem(this.drawingList[len - 1]);
        }
      });
    },
    generateCode() {
      this.AssembleFormData();
      const script = vueScript(makeUpJs(this.formData));
      const html = vueTemplate(makeUpHtml(this.formData));
      const css = cssStyle(makeUpCss(this.formData));
      return beautifier.html(html + script + css, beautifierConf.html);
    },
    download() {
      this.execDownload();
    },
    tagChange(newTag) {
      newTag = this.cloneComponent(newTag);
      newTag.fieldName = this.activeData.fieldName;
      newTag.formId = this.activeId;
      newTag.span = this.activeData.span;
      delete this.activeData.tag;
      delete this.activeData.fieldType;
      delete this.activeData.document;
      Object.keys(newTag).forEach((key) => {
        if (
          this.activeData[key] !== undefined &&
          typeof this.activeData[key] === typeof newTag[key]
        ) {
          newTag[key] = this.activeData[key];
        }
      });
      this.activeData = newTag;
      this.updateDrawingList(newTag, this.drawingList);
    },
    updateDrawingList(newTag, list) {
      const index = list.findIndex((item) => item.formId === this.activeId);
      if (index > -1) {
        list.splice(index, 1, newTag);
      } else {
        list.forEach((item) => {
          if (Array.isArray(item.children))
            this.updateDrawingList(newTag, item.children);
        });
      }
    },
  },
};
</script>

<style lang='scss'>
body,
html {
  margin: 0;
  padding: 0;
  background: #fff;
  -moz-osx-font-smoothing: grayscale;
  -webkit-font-smoothing: antialiased;
  text-rendering: optimizeLegibility;
  font-family: -apple-system, BlinkMacSystemFont, Segoe UI, Helvetica, Arial,
    sans-serif, Apple Color Emoji, Segoe UI Emoji;
}

input,
textarea {
  font-family: -apple-system, BlinkMacSystemFont, Segoe UI, Helvetica, Arial,
    sans-serif, Apple Color Emoji, Segoe UI Emoji;
}

.editor-tabs {
  background: #121315;
  .el-tabs__header {
    margin: 0;
    border-bottom-color: #121315;
    .el-tabs__nav {
      border-color: #121315;
    }
  }
  .el-tabs__item {
    height: 32px;
    line-height: 32px;
    color: #888a8e;
    border-left: 1px solid #121315 !important;
    background: #363636;
    margin-right: 5px;
    user-select: none;
  }
  .el-tabs__item.is-active {
    background: #1e1e1e;
    border-bottom-color: #1e1e1e !important;
    color: #fff;
  }
  .el-icon-edit {
    color: #f1fa8c;
  }
  .el-icon-document {
    color: #a95812;
  }
}

// home
.right-scrollbar {
  .el-scrollbar__view {
    padding: 12px 18px 15px 15px;
  }
}
.left-scrollbar .el-scrollbar__wrap {
  box-sizing: border-box;
  overflow-x: hidden !important;
  margin-bottom: 0 !important;
}
.center-tabs {
  .el-tabs__header {
    margin-bottom: 0 !important;
  }
  .el-tabs__item {
    width: 50%;
    text-align: center;
  }
  .el-tabs__nav {
    width: 100%;
  }
}
.reg-item {
  padding: 12px 6px;
  background: #f8f8f8;
  position: relative;
  border-radius: 4px;
  .close-btn {
    position: absolute;
    right: -6px;
    top: -6px;
    display: block;
    width: 16px;
    height: 16px;
    line-height: 16px;
    background: rgba(0, 0, 0, 0.2);
    border-radius: 50%;
    color: #fff;
    text-align: center;
    z-index: 1;
    cursor: pointer;
    font-size: 12px;
    &:hover {
      background: rgba(210, 23, 23, 0.5);
    }
  }
  & + .reg-item {
    margin-top: 18px;
  }
}
.action-bar {
  & .el-button + .el-button {
    margin-left: 15px;
  }
  & i {
    font-size: 20px;
    vertical-align: middle;
    position: relative;
    top: -1px;
  }
}

.custom-tree-node {
  width: 100%;
  font-size: 14px;
  .node-operation {
    float: right;
  }
  i[class*="el-icon"] + i[class*="el-icon"] {
    margin-left: 6px;
  }
  .el-icon-plus {
    color: #409eff;
  }
  .el-icon-delete {
    color: #157a0c;
  }
}

.left-scrollbar .el-scrollbar__view {
  overflow-x: hidden;
}

.el-rate {
  display: inline-block;
  vertical-align: text-top;
}
.el-upload__tip {
  line-height: 1.2;
}

$selectedColor: #f6f7ff;
$lighterBlue: #409eff;

.container {
  position: relative;
  width: 100%;
  height: 100%;
}

.components-list {
  padding: 8px;
  box-sizing: border-box;
  height: 100%;
  .components-item {
    display: inline-block;
    width: 48%;
    margin: 1%;
    transition: transform 0ms !important;
  }
}
.components-draggable {
  padding-bottom: 20px;
}
.components-title {
  font-size: 14px;
  color: #222;
  margin: 6px 2px;
  .svg-icon {
    color: #666;
    font-size: 18px;
  }
}

.components-body {
  padding: 8px 10px;
  background: $selectedColor;
  font-size: 12px;
  cursor: move;
  border: 1px dashed $selectedColor;
  border-radius: 3px;
  .svg-icon {
    color: #777;
    font-size: 15px;
  }
  &:hover {
    border: 1px dashed #787be8;
    color: #787be8;
    .svg-icon {
      color: #787be8;
    }
  }
}

.left-board {
  width: 260px;
  position: absolute;
  left: 0;
  top: 0;
  height: 100vh;
}
.left-scrollbar {
  height: calc(100vh - 42px);
  overflow: hidden;
}
.center-scrollbar {
  height: calc(100vh - 42px);
  overflow: hidden;
  border-left: 1px solid #f1e8e8;
  border-right: 1px solid #f1e8e8;
  box-sizing: border-box;
}
.center-board {
  height: 100vh;
  width: auto;
  margin: 0 350px 0 260px;
  box-sizing: border-box;
}
.empty-info {
  position: absolute;
  top: 46%;
  left: 0;
  right: 0;
  text-align: center;
  font-size: 18px;
  color: #ccb1ea;
  letter-spacing: 4px;
}
.action-bar {
  position: relative;
  height: 42px;
  text-align: right;
  padding: 0 15px;
  box-sizing: border-box;
  border: 1px solid #f1e8e8;
  border-top: none;
  border-left: none;
  .delete-btn {
    color: #f56c6c;
  }
}

.center-board-row {
  padding: 12px 12px 15px 12px;
  box-sizing: border-box;
  & > .el-form {
    // 69 = 12+15+42
    height: calc(100vh - 69px);
  }
}
.drawing-board {
  height: 100%;
  position: relative;
  .components-body {
    padding: 0;
    margin: 0;
    font-size: 0;
  }
  .sortable-ghost {
    position: relative;
    display: block;
    overflow: hidden;
    &::before {
      content: " ";
      position: absolute;
      left: 0;
      right: 0;
      top: 0;
      height: 3px;
      background: rgb(89, 89, 223);
      z-index: 2;
    }
  }
  .components-item.sortable-ghost {
    width: 100%;
    height: 60px;
    background-color: $selectedColor;
  }
  .active-from-item {
    & > .el-form-item {
      background: $selectedColor;
      border-radius: 6px;
    }
    & > .drawing-item-copy,
    & > .drawing-item-delete {
      display: initial;
    }
    & > .component-name {
      color: $lighterBlue;
    }
  }
  .el-form-item {
    margin-bottom: 15px;
  }
}
.drawing-item {
  position: relative;
  cursor: move;
  &.unfocus-bordered:not(.activeFromItem) > div:first-child {
    border: 1px dashed #ccc;
  }
  .el-form-item {
    padding: 12px 10px;
  }
}
.drawing-row-item {
  position: relative;
  cursor: move;
  box-sizing: border-box;
  border: 1px dashed #ccc;
  border-radius: 3px;
  padding: 0 2px;
  margin-bottom: 15px;
  .drawing-row-item {
    margin-bottom: 2px;
  }
  .el-col {
    margin-top: 22px;
  }
  .el-form-item {
    margin-bottom: 0;
  }
  .drag-wrapper {
    min-height: 80px;
  }
  &.active-from-item {
    border: 1px dashed $lighterBlue;
  }
  .component-name {
    position: absolute;
    top: 0;
    left: 0;
    font-size: 12px;
    color: #bbb;
    display: inline-block;
    padding: 0 6px;
  }
}
.drawing-item,
.drawing-row-item {
  &:hover {
    & > .el-form-item {
      background: $selectedColor;
      border-radius: 6px;
    }
    & > .drawing-item-copy,
    & > .drawing-item-delete {
      display: initial;
    }
  }
  & > .drawing-item-copy,
  & > .drawing-item-delete {
    display: none;
    position: absolute;
    top: -10px;
    width: 22px;
    height: 22px;
    line-height: 22px;
    text-align: center;
    border-radius: 50%;
    font-size: 12px;
    border: 1px solid;
    cursor: pointer;
    z-index: 1;
  }
  & > .drawing-item-copy {
    right: 56px;
    border-color: $lighterBlue;
    color: $lighterBlue;
    background: #fff;
    &:hover {
      background: $lighterBlue;
      color: #fff;
    }
  }
  & > .drawing-item-delete {
    right: 24px;
    border-color: #f56c6c;
    color: #f56c6c;
    background: #fff;
    &:hover {
      background: #f56c6c;
      color: #fff;
    }
  }
}
</style>
