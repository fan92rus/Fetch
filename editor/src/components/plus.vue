<template>
  <div class="inline flex">
    <div v-if="!isAdd" class="plus inline" @click="isAdd = !isAdd">+</div>
    <div v-else class="plus inline" @click="isAdd = !isAdd">-</div>
    <b-field v-show="isAdd" class="m-0">
      <b-input v-model="addText" size="is-small"></b-input>
      <b-button @click="Add" size="is-small">Add</b-button>
    </b-field>
  </div>
</template>

<script>
export default {
  props: {
    parent: {
      require: true,
    },
  },
  data() {
    return {
      addText: "",
      isAdd: false,
    };
  },
  methods: {
    Add() {
      console.log(this.parent);
      if (this.addText != "") {
        this.parent.Children.push({
          Property: this.addText,
          Children: [],
        });
        console.log(this.parent);
      }
    },
  },
};
</script>

<style lang="scss">
.inline {
  display: inline-block;
}
.plus {
  cursor: pointer;
  margin-left: 10px;
  margin-right: 10px;
  &:hover {
    transition-duration: 0.3s;
    font-weight: bold;
  }
}
.m-0 {
  margin: 0 !important;
}
</style>